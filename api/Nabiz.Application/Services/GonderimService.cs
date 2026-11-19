using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IGonderimService
{
    /// <summary>Vakti gelen planları işler; üretilen gönderim sayısını döner (zamanlayıcı çağırır).</summary>
    Task<int> VadesiGelenleriIsleAsync(CancellationToken ct);
    /// <summary>Planı zamanına bakmadan hemen gönderir (deneme/test).</summary>
    Task<GonderimSonucu> PlaniSimdiGonderAsync(int planId, CancellationToken ct);
    /// <summary>Plansız, tek seferlik gönderim: seçilen paketi hedef kitleye hemen yollar.</summary>
    Task<AnlikGonderYanit> AnlikGonderAsync(AnlikGonderIstek istek, CancellationToken ct);
    /// <summary>E-postası gitmeyen gönderimleri aynı linkle yeniden dener.</summary>
    Task<GonderimSonucu> HatalilariTekrarDeneAsync(int partiId, CancellationToken ct);

    /// <summary>Çalışma saatlerinde, cevaplamayanlara (en fazla N kez) nazik hatırlatma maili yollar. Gönderilen sayıyı döner.</summary>
    Task<int> HatirlatmalariGonderAsync(CancellationToken ct);

    Task<List<PartiDto>> PartilerAsync(CancellationToken ct);
    Task<PartiDetayDto> PartiDetayAsync(int partiId, CancellationToken ct);
    Task<byte[]> PartiCsvAsync(int partiId, CancellationToken ct);
}

public class GonderimService(IUnitOfWork uow, IEmailSender mail, IZaman zaman, IUygulamaAyarlari uygulama, ICurrentUser kullanici, IAyarService ayarlar, IDenetimService denetim, ILogger<GonderimService> log, IDilBilgisi? dilBilgisi = null) : IGonderimService
{
    private string Lang => dilBilgisi?.Dil ?? "tr";

    private IQueryable<SoruPaketi> PaketSorgu => uow.Paketler.Query.Include(p => p.Sorular).ThenInclude(s => s.Soru);

    // ---------------- Gönderim ----------------

    public async Task<int> VadesiGelenleriIsleAsync(CancellationToken ct)
    {
        var simdi = zaman.Simdi;
        var planlar = await uow.GonderimPlanlari.Query.Where(p => p.Aktif).ToListAsync(ct);
        var toplam = 0;
        foreach (var p in planlar.Where(p => Vadesi(p, simdi)))
        {
            var sonuc = await PlanGonderAsync(p, ct);
            log.LogInformation("Plan {PlanId} işlendi: {Basarili}/{Toplam} gönderildi.", p.Id, sonuc.Basarili, sonuc.Toplam);
            toplam += sonuc.Toplam;
        }
        return toplam;
    }

    public async Task<GonderimSonucu> PlaniSimdiGonderAsync(int planId, CancellationToken ct)
    {
        var p = await uow.GonderimPlanlari.FindAsync(planId, ct) ?? throw new BulunamadiHatasi();
        var sonuc = await PlanGonderAsync(p, ct);
        await denetim.KaydetAsync("Plan şimdi gönderildi", $"Plan #{planId} ({sonuc.Basarili}/{sonuc.Toplam} mail)", ct);
        return sonuc;
    }

    public async Task<AnlikGonderYanit> AnlikGonderAsync(AnlikGonderIstek i, CancellationToken ct)
    {
        var paket = await PaketSorgu.FirstOrDefaultAsync(p => p.Id == i.PaketId, ct) ?? throw new DogrulamaHatasi("Paket bulunamadı.");
        await HedefYardimcisi.DogrulaAsync(uow, i.HedefTipi, i.HedefDeger, ct);
        var hedef = i.HedefTipi == HedefTipi.Hepsi ? null : i.HedefDeger;
        var (parti, sonuc) = await GonderAsync(paket, i.HedefTipi, hedef, planId: null, anlik: true, ct);
        await denetim.KaydetAsync("Anlık gönderim", $"{paket.AdGoster("tr")} → {i.HedefTipi} ({sonuc.Basarili}/{sonuc.Toplam} mail)", ct);
        return new AnlikGonderYanit(parti.Id, sonuc);
    }

    public async Task<GonderimSonucu> HatalilariTekrarDeneAsync(int partiId, CancellationToken ct)
    {
        var parti = await uow.GonderimPartileri.FindAsync(partiId, ct) ?? throw new BulunamadiHatasi();
        var paket = await PaketSorgu.FirstAsync(p => p.Id == parti.PaketId, ct);
        var hatali = await uow.Gonderimler.Query.Include(g => g.Kullanici).Where(g => g.PartiId == partiId && g.Durum == GonderimDurumu.Hata).ToListAsync(ct);
        var sonuc = await PostaGonderAsync(hatali.Select(g => (g, g.Kullanici!)).ToList(), paket, ct);
        await uow.SaveChangesAsync(ct);
        return sonuc;
    }

    public async Task<int> HatirlatmalariGonderAsync(CancellationToken ct)
    {
        if (!uygulama.HatirlatmaAktif) return 0;
        var simdi = zaman.Simdi;
        if (simdi.Hour < 9 || simdi.Hour >= 18 || simdi.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return 0;   // çalışma saatleri dışında rahatsız etme

        var esik = zaman.UtcNow.AddDays(-Math.Max(1, uygulama.HatirlatmaGun));
        var maks = uygulama.HatirlatmaMaks;
        var adaylar = await uow.Gonderimler.Query
            .Include(x => x.Kullanici).Include(x => x.Parti!).ThenInclude(p => p.Paket!).ThenInclude(p => p.Sorular).ThenInclude(s => s.Soru)
            .Where(x => x.Durum == GonderimDurumu.Gonderildi && !x.Tamamlandi && x.HatirlatmaSayisi < maks
                        && !x.Parti!.Demo && !x.Parti.Test && x.Kullanici!.Aktif
                        && (x.SonHatirlatma ?? x.GonderimZamani) <= esik)
            .OrderBy(x => x.GonderimZamani).Take(200).ToListAsync(ct);

        var gonderilen = 0;
        foreach (var x in adaylar)
        {
            try
            {
                var (konu, html) = EmailSablonu.Hatirlatma(x.Kullanici!, x.Parti!.Paket!, $"{uygulama.PublicUrl.TrimEnd('/')}/cevap/{x.Token}");
                await mail.GonderAsync(x.Kullanici!.Email, konu, html, ct);
                x.HatirlatmaSayisi++; x.SonHatirlatma = zaman.UtcNow; gonderilen++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                x.SonHatirlatma = zaman.UtcNow;   // hatalıyı da bir süre bekletelim; her dakika yeniden denemesin
                log.LogWarning("Hatırlatma {Email} adresine gitmedi: {Mesaj}", x.Kullanici!.Email, ex.GetBaseException().Message);
            }
        }
        if (adaylar.Count > 0) await uow.SaveChangesAsync(ct);
        return gonderilen;
    }

    /// <summary>Plan bugün çalışmalı mı? Başlangıç geldi, saat geçti ve son çalışmadan beri yeterli gün geçti ("bir kez" planı yalnızca bir kez çalışır).</summary>
    internal static bool Vadesi(GonderimPlani p, DateTime simdi)
    {
        if (p.Baslangic.Date > simdi.Date || simdi.TimeOfDay < p.Saat) return false;
        if (p.SonCalisma is null) return true;
        // SiklikGun = 0: "bir kez" planı; bir kez çalıştıktan sonra tekrar çalışmaz.
        return p.SiklikGun > 0 && (simdi.Date - p.SonCalisma.Value.Date).Days >= p.SiklikGun;
    }

    private async Task<GonderimSonucu> PlanGonderAsync(GonderimPlani plan, CancellationToken ct)
    {
        var paket = await PaketSorgu.FirstOrDefaultAsync(p => p.Id == plan.PaketId, ct) ?? throw new BulunamadiHatasi("Paket bulunamadı.");
        // Plan çalıştı olarak işaretlenir; böylece aynı gün tekrar tekrar gönderilmez.
        plan.SonCalisma = zaman.Simdi;
        if (plan.SiklikGun == 0) plan.Aktif = false;   // bir kez gönderilecek plan tamamlandı
        var (_, sonuc) = await GonderAsync(paket, plan.HedefTipi, plan.HedefDeger, plan.Id, anlik: false, ct);
        return sonuc;
    }

    private async Task<(GonderimPartisi Parti, GonderimSonucu Sonuc)> GonderAsync(
        SoruPaketi paket, HedefTipi tip, string? hedefDeger, int? planId, bool anlik, CancellationToken ct)
    {
        var kullanicilar = await HedefYardimcisi.KullanicilariGetirAsync(uow, tip, hedefDeger, ct);
        if (kullanicilar.Count == 0) throw new DogrulamaHatasi("Hedef kitlede gönderilecek aktif kullanıcı yok.");

        var parti = new GonderimPartisi
        {
            PaketId = paket.Id, HedefTipi = tip, HedefDeger = hedefDeger, Zaman = zaman.UtcNow,
            PlanId = planId, Anlik = anlik, OlusturanKullaniciId = anlik ? kullanici.IdVeyaNull : null,
        };
        var gonderimler = kullanicilar.Select(k => (G: new Gonderim
        {
            Parti = parti, KullaniciId = k.Id, GonderimZamani = zaman.UtcNow,
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant(),
        }, K: k)).ToList();
        foreach (var (g, _) in gonderimler) uow.Gonderimler.Add(g);
        await uow.SaveChangesAsync(ct);   // önce kayıtlar yazılsın; mail hatası kayıtları etkilemesin

        var sonuc = await PostaGonderAsync(gonderimler, paket, ct);
        await uow.SaveChangesAsync(ct);
        return (parti, sonuc);
    }

    private async Task<GonderimSonucu> PostaGonderAsync(List<(Gonderim G, Kullanici K)> liste, SoruPaketi paket, CancellationToken ct)
    {
        int basarili = 0, hatali = 0; string? ilkHata = null;
        foreach (var (g, k) in liste)
        {
            try
            {
                var (konu, html) = EmailSablonu.Paket(k, paket, $"{uygulama.PublicUrl.TrimEnd('/')}/cevap/{g.Token}");
                await mail.GonderAsync(k.Email, konu, html, ct);
                g.Durum = GonderimDurumu.Gonderildi; g.Hata = null; basarili++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var mesaj = ex.GetBaseException().Message;
                g.Durum = GonderimDurumu.Hata; g.Hata = mesaj.Length > 480 ? mesaj[..480] : mesaj;
                ilkHata ??= mesaj; hatali++;
                log.LogWarning("{Email} adresine e-posta gönderilemedi: {Mesaj}", k.Email, mesaj);
            }
        }
        return new GonderimSonucu(liste.Count, basarili, hatali, ilkHata);
    }

    // ---------------- Gönderim raporları ----------------

    public async Task<List<PartiDto>> PartilerAsync(CancellationToken ct)
    {
        var partiler = await uow.GonderimPartileri.Query.Include(p => p.Paket!).ThenInclude(p => p.Sorular)
            .OrderByDescending(p => p.Zaman).Take(100).ToListAsync(ct);
        var ids = partiler.Select(p => p.Id).ToList();
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Where(g => ids.Contains(g.PartiId)).ToListAsync(ct);
        var sorular = await uow.Sorular.Query.ToDictionaryAsync(s => s.Id, ct);
        var isimler = await IsimleriAsync(partiler, ct);
        return partiler.Select(p => Ozet(p, gonderimler.Where(g => g.PartiId == p.Id).ToList(), sorular, isimler)).ToList();
    }

    public async Task<PartiDetayDto> PartiDetayAsync(int partiId, CancellationToken ct)
    {
        var parti = await uow.GonderimPartileri.Query.Include(p => p.Paket!).ThenInclude(p => p.Sorular).ThenInclude(s => s.Soru)
            .FirstOrDefaultAsync(p => p.Id == partiId, ct) ?? throw new BulunamadiHatasi();
        await denetim.KaydetAsync("Gönderim raporu görüntülendi", $"#{partiId} {parti.Paket!.AdGoster("tr")}", ct);
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Include(g => g.Kullanici).Where(g => g.PartiId == partiId).ToListAsync(ct);
        var a = await ayarlar.GetirAsync(ct);
        var sorular = parti.Paket!.Sorular.OrderBy(s => s.Sira).Select(s => s.Soru!).ToList();
        var soruSozluk = sorular.ToDictionary(s => s.Id);
        var tamam = gonderimler.Where(g => g.Tamamlandi).ToList();

        var soruDagilimlari = sorular.Select(s =>
        {
            var cevaplar = tamam.SelectMany(g => g.Cevaplar).Where(c => c.SoruId == s.Id).ToList();
            return new SoruDagilimi(s.Id, s.Metin(Lang), s.Tip, cevaplar.Count,
                cevaplar.Count == 0 ? null : Math.Round(cevaplar.Average(c => s.Skor(c.Deger)), 1),
                s.Degerler().Select(d => new DagilimNoktasi(d, cevaplar.Count(c => c.Deger == d))).ToList());
        }).ToList();

        var kisiler = gonderimler.Select(g =>
        {
            var k = g.Kullanici!;
            var cevaplar = g.Tamamlandi
                ? sorular.Select(s => g.Cevaplar.FirstOrDefault(c => c.SoruId == s.Id)).Where(c => c is not null).Select(c =>
                {
                    var s = soruSozluk[c!.SoruId];
                    return new KisiCevabi(s.Id, s.Metin(Lang), s.Tip, c.Deger, Math.Round(s.Skor(c.Deger), 0), c.YorumAnonim ? null : c.Yorum, c.YorumAnonim && !string.IsNullOrWhiteSpace(c.Yorum), s.DusukMu(c.Deger, a.EmojiEsik, a.PuanEsik));
                }).ToList()
                : [];
            return new PartiKisiSatiri(k.Id, k.Ad, k.Email, k.Departman, g.Durum.ToString(), g.Hata, g.Tamamlandi,
                cevaplar.Count == 0 ? null : Math.Round(cevaplar.Average(c => c.Skor), 0), cevaplar.Any(c => c.Dusuk),
                g.TamamlanmaZamani is null ? null : zaman.Yerel(g.TamamlanmaZamani.Value), cevaplar);
        }).OrderByDescending(s => s.Tamamlandi).ThenBy(s => s.OrtalamaSkor ?? 999).ThenBy(s => s.Ad).ToList();

        var anonim = tamam.SelectMany(x => x.Cevaplar).Where(c => c.YorumAnonim && !string.IsNullOrWhiteSpace(c.Yorum))
            .Select(c => new AnonimYorumSatiri(soruSozluk[c.SoruId].Metin(Lang), c.Yorum!)).ToList();
        var isimler = await IsimleriAsync([parti], ct);
        return new PartiDetayDto(Ozet(parti, gonderimler, soruSozluk, isimler), soruDagilimlari, kisiler, anonim);
    }

    public async Task<byte[]> PartiCsvAsync(int partiId, CancellationToken ct)
    {
        var d = await PartiDetayAsync(partiId, ct);
        await denetim.KaydetAsync("Gönderim CSV indirildi", $"#{partiId}", ct);
        var satirlar = new List<object?[]> { new object?[] { "Kişi", "E-posta", "Departman", "Mail durumu", "Cevapladı", "Ort. skor", "Soru", "Cevap", "Skor", "Yorum" } };
        foreach (var k in d.Kisiler)
        {
            if (k.Cevaplar.Count == 0) satirlar.Add([k.Ad, k.Email, k.Departman, k.GonderimDurumu, k.Tamamlandi, k.OrtalamaSkor, null, null, null, null]);
            foreach (var c in k.Cevaplar)
                satirlar.Add([k.Ad, k.Email, k.Departman, k.GonderimDurumu, k.Tamamlandi, k.OrtalamaSkor, c.Soru, c.Deger, c.Skor, c.YorumAnonim ? "(anonim yorum)" : c.Yorum]);
        }
        foreach (var a in d.AnonimYorumlar) satirlar.Add(["(anonim)", null, null, null, true, null, a.Soru, null, null, a.Yorum]);
        return Csv.Uret(satirlar);
    }

    private PartiDto Ozet(GonderimPartisi p, List<Gonderim> g, IReadOnlyDictionary<int, Soru> sorular, IReadOnlyDictionary<int, string> isimler)
    {
        var tamam = g.Where(x => x.Tamamlandi).ToList();
        var skorlar = tamam.SelectMany(x => x.Cevaplar).Where(c => sorular.ContainsKey(c.SoruId)).Select(c => sorular[c.SoruId].Skor(c.Deger)).ToList();
        return new PartiDto(p.Id, zaman.Yerel(p.Zaman), p.PaketId, p.Paket!.AdGoster(Lang), p.Paket.Sorular.Count,
            HedefYardimcisi.Aciklama(p.HedefTipi, p.HedefDeger, isimler, Lang), p.Anlik, p.Demo, p.Test, g.Count, g.Count(x => x.Durum == GonderimDurumu.Hata),
            tamam.Count, g.Count == 0 ? 0 : Math.Round(tamam.Count * 100.0 / g.Count, 1),
            skorlar.Count == 0 ? null : Math.Round(skorlar.Average(), 1));
    }

    private async Task<Dictionary<int, string>> IsimleriAsync(IEnumerable<GonderimPartisi> partiler, CancellationToken ct)
    {
        var ids = partiler.Where(p => p.HedefTipi == HedefTipi.Kisi).SelectMany(p => Domain.Hedef.KisiIdleri(p.HedefDeger)).Distinct().ToList();
        return ids.Count == 0 ? [] : await uow.Kullanicilar.Query.Where(k => ids.Contains(k.Id)).ToDictionaryAsync(k => k.Id, k => k.Ad, ct);
    }
}
