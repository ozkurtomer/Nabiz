using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IRaporService
{
    Task<RaporDto> GetirAsync(RaporFiltre filtre, CancellationToken ct);
    Task<List<CevapSatiri>> CevaplarAsync(RaporFiltre filtre, bool sadeceYorumlu, CancellationToken ct);
    Task<byte[]> CevaplarCsvAsync(RaporFiltre filtre, CancellationToken ct);
    /// <summary>Bir çalışanın zaman içindeki skorları ve cevapları (yalnızca İK).</summary>
    Task<KisiGecmisiDto> KisiGecmisiAsync(int kullaniciId, CancellationToken ct);
}

/// <summary>
/// Raporları yalnızca İK görür; kişi bazlı veriye her erişim denetim kaydına işlenir.
/// Çalışan yorumunu anonim bıraktıysa İK dahil kimse o yorumun sahibini göremez.
/// Raporlar yalnızca çalışanın "Tamamla" dediği gönderimleri sayar.
/// </summary>
public class RaporService(IUnitOfWork uow, ICurrentUser me, IZaman zaman, IAyarService ayarlar, IDenetimService denetim, IDilBilgisi? dilBilgisi = null) : IRaporService
{
    private string Lang => dilBilgisi?.Dil ?? "tr";

    private record Satir(Gonderim G, Cevap C, Soru S, double Skor);

    private record Baglam(
        DateTime Bas, DateTime Bit, AyarlarDto Ayar, string Kapsam,
        Dictionary<int, Kullanici> Kullanicilar, Dictionary<int, Soru> Sorular, List<Gonderim> Gonderimler)
    {
        public List<Gonderim> Tamam => Gonderimler.Where(g => g.Tamamlandi).ToList();
        public string Departman(Gonderim g) => Kullanicilar[g.KullaniciId].Departman ?? "—";
        public List<Satir> Satirlar(IEnumerable<Gonderim> gonderimler) =>
            gonderimler.SelectMany(g => g.Cevaplar.Select(c => new Satir(g, c, Sorular[c.SoruId], Sorular[c.SoruId].Skor(c.Deger)))).ToList();
    }

    public async Task<RaporDto> GetirAsync(RaporFiltre f, CancellationToken ct)
    {
        var b = await YukleAsync(f, ct);
        var tamam = b.Tamam;

        // Önceki dönem (aynı uzunlukta, hemen öncesi): "geçen döneme göre" karşılaştırması için
        var gun = (b.Bit - b.Bas).Days + 1;
        var onceki = await YukleAsync(f with { Baslangic = b.Bas.AddDays(-gun), Bitis = b.Bas.AddDays(-1) }, ct);
        var oncekiTamam = onceki.Tamam;
        RaporOzet? oncekiOzet = oncekiTamam.Count > 0 ? Ozet(onceki.Gonderimler.Count, oncekiTamam.Count, onceki.Satirlar(oncekiTamam), onceki.Ayar) : null;
        var oncekiDep = oncekiTamam.GroupBy(onceki.Departman).ToDictionary(x => x.Key, x => Math.Round(onceki.Satirlar(x).Average(s => s.Skor), 1));

        var departmanlar = b.Gonderimler.GroupBy(b.Departman).Select(grp =>
        {
            var t = grp.Where(g => g.Tamamlandi).ToList();
            var o = Ozet(grp.Count(), t.Count, b.Satirlar(t), b.Ayar);
            return new DepartmanSatiri(grp.Key, o.Gonderilen, o.Cevaplayan, o.CevapOrani, o.OrtalamaSkor, o.Olumlu, o.Notr, o.Olumsuz,
                oncekiDep.TryGetValue(grp.Key, out var oe) ? oe : null);
        }).OrderByDescending(d => d.OrtalamaSkor ?? -1).ToList();

        var satirlar = b.Satirlar(tamam);
        var sorular = satirlar.GroupBy(x => x.S).Select(grp =>
        {
            var s = grp.Key;
            var dagilim = s.Degerler().Select(d => new DagilimNoktasi(d, grp.Count(x => x.C.Deger == d))).ToList();
            return new SoruSatiri(s.Id, s.Metin(Lang), s.Tip, grp.Count(), Math.Round(grp.Average(x => x.Skor), 1), dagilim);
        }).OrderByDescending(s => s.CevapSayisi).ToList();

        var gunluk = tamam.GroupBy(g => zaman.Yerel(g.GonderimZamani).Date).ToDictionary(x => x.Key, x => x.ToList());
        var trend = new List<TrendNoktasi>();
        for (var d = b.Bas; d <= b.Bit; d = d.AddDays(1))
        {
            gunluk.TryGetValue(d, out var liste);
            trend.Add(new TrendNoktasi(d, liste is null ? null : Math.Round(b.Satirlar(liste).Average(x => x.Skor), 1), liste?.Count ?? 0));
        }

        return new RaporDto(b.Bas, b.Bit, b.Kapsam, Ozet(b.Gonderimler.Count, tamam.Count, satirlar, b.Ayar), oncekiOzet, trend, departmanlar, sorular);
    }

    public async Task<List<CevapSatiri>> CevaplarAsync(RaporFiltre f, bool sadeceYorumlu, CancellationToken ct)
    {
        var b = await YukleAsync(f, ct);
        await denetim.KaydetAsync("Cevap listesi görüntülendi", $"{b.Bas:dd.MM.yyyy}-{b.Bit:dd.MM.yyyy} · {b.Kapsam}", ct);

        var satirlar = b.Satirlar(b.Tamam).Where(x => !sadeceYorumlu || !string.IsNullOrWhiteSpace(x.C.Yorum));
        var paketAd = await uow.GonderimPartileri.Query.Include(p => p.Paket).ToDictionaryAsync(p => p.Id, p => p.Paket!.AdGoster(Lang), ct);

        return satirlar.OrderByDescending(x => x.C.GuncellemeZamani).Take(500).Select(x =>
        {
            var k = b.Kullanicilar[x.G.KullaniciId];
            var gizle = x.C.YorumAnonim && !string.IsNullOrWhiteSpace(x.C.Yorum);   // anonim yorumun sahibi kimseye gösterilmez
            return new CevapSatiri(x.C.Id, zaman.Yerel(x.C.GuncellemeZamani),
                gizle ? null : k.Ad, gizle ? null : k.Departman,
                paketAd.GetValueOrDefault(x.G.PartiId, "—"), x.S.Metin(Lang), x.S.Tip, x.C.Deger, Math.Round(x.Skor, 0), x.C.Yorum,
                x.S.DusukMu(x.C.Deger, b.Ayar.EmojiEsik, b.Ayar.PuanEsik));
        }).ToList();
    }

    public async Task<byte[]> CevaplarCsvAsync(RaporFiltre f, CancellationToken ct)
    {
        var liste = await CevaplarAsync(f, sadeceYorumlu: false, ct);
        await denetim.KaydetAsync("Rapor CSV indirildi", $"{liste.Count} satır", ct);
        var satirlar = new List<object?[]> { new object?[] { "Tarih", "Kişi", "Departman", "Paket", "Soru", "Cevap", "Skor", "Düşük", "Yorum" } };
        satirlar.AddRange(liste.Select(c => new object?[] { c.Tarih, c.Kisi, c.Departman, c.Paket, c.Soru, c.Deger, c.Skor, c.Dusuk, c.Yorum }));
        return Csv.Uret(satirlar);
    }

    public async Task<KisiGecmisiDto> KisiGecmisiAsync(int kullaniciId, CancellationToken ct)
    {
        await IkMiAsync(ct);
        var k = await uow.Kullanicilar.FindAsync(kullaniciId, ct) ?? throw new BulunamadiHatasi();
        await denetim.KaydetAsync("Kişi geçmişi görüntülendi", k.Ad, ct);

        var ayar = await ayarlar.GetirAsync(ct);
        var sorular = await uow.Sorular.Query.ToDictionaryAsync(s => s.Id, ct);
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Include(g => g.Parti!).ThenInclude(p => p.Paket)
            .Where(g => g.KullaniciId == kullaniciId && g.Tamamlandi && !g.Parti!.Test).OrderBy(g => g.TamamlanmaZamani).ToListAsync(ct);

        var noktalar = gonderimler.Select(g =>
        {
            var skorlar = g.Cevaplar.Select(c => sorular[c.SoruId].Skor(c.Deger)).ToList();
            return new KisiNoktasi(zaman.Yerel(g.TamamlanmaZamani!.Value), g.Parti!.Paket!.AdGoster(Lang), Math.Round(skorlar.Average(), 1),
                g.Cevaplar.Any(c => sorular[c.SoruId].DusukMu(c.Deger, ayar.EmojiEsik, ayar.PuanEsik)));
        }).ToList();

        var cevaplar = gonderimler.SelectMany(g => g.Cevaplar.Select(c => (G: g, C: c))).OrderByDescending(x => x.C.GuncellemeZamani).Take(200)
            .Select(x =>
            {
                var s = sorular[x.C.SoruId];
                return new KisiGecmisCevap(zaman.Yerel(x.C.GuncellemeZamani), x.G.Parti!.Paket!.AdGoster(Lang), s.Metin(Lang), s.Tip, x.C.Deger, Math.Round(s.Skor(x.C.Deger), 0),
                    x.C.YorumAnonim ? null : x.C.Yorum, s.DusukMu(x.C.Deger, ayar.EmojiEsik, ayar.PuanEsik));
            }).ToList();

        // Düşüşte: son 3 ölçüm sürekli azalıyorsa ya da hepsi düşük (<40) ise
        var son3 = noktalar.TakeLast(3).Select(n => n.Skor).ToList();
        var dususte = son3.Count == 3 && ((son3[0] > son3[1] && son3[1] > son3[2]) || son3.All(s => s < 40));

        return new KisiGecmisiDto(k.Id, k.Ad, k.Email, k.Departman, noktalar.Count == 0 ? null : Math.Round(noktalar.Average(n => n.Skor), 1), dususte, noktalar, cevaplar);
    }

    private async Task IkMiAsync(CancellationToken ct)
    {
        var ben = await uow.Kullanicilar.FindAsync(me.Id, ct) ?? throw new YetkisizHatasi();
        if (ben.Rol != Rol.IK) throw new YetkisizHatasi("Raporları ve kişi bazlı verileri yalnızca İK görebilir.");
    }

    private async Task<Baglam> YukleAsync(RaporFiltre f, CancellationToken ct)
    {
        await IkMiAsync(ct);

        var bit = (f.Bitis ?? zaman.Simdi).Date;
        var bas = (f.Baslangic ?? bit.AddDays(-29)).Date;
        if (bas > bit) throw new DogrulamaHatasi("Başlangıç tarihi bitişten sonra olamaz.");
        if ((bit - bas).TotalDays > 400) throw new DogrulamaHatasi("En fazla 400 günlük aralık seçilebilir.");

        IQueryable<Kullanici> kq = uow.Kullanicilar.Query;
        if (!string.IsNullOrWhiteSpace(f.Departman)) kq = kq.Where(k => k.Departman == f.Departman);

        var kullanicilar = await kq.ToDictionaryAsync(k => k.Id, ct);
        var basUtc = zaman.UtcCevir(bas);
        var bitUtc = zaman.UtcCevir(bit.AddDays(1));
        var ids = kq.Select(k => k.Id);
        var gq = uow.Gonderimler.Query.Include(g => g.Cevaplar).Include(g => g.Parti)
            .Where(g => g.GonderimZamani >= basUtc && g.GonderimZamani < bitUtc && ids.Contains(g.KullaniciId) && !g.Parti!.Test);
        if (f.PaketId is int pid) gq = gq.Where(g => g.Parti!.PaketId == pid);

        var ayar = await ayarlar.GetirAsync(ct);
        var kapsam = string.IsNullOrWhiteSpace(f.Departman) ? (Lang == "en" ? "Whole company" : "Tüm şirket") : f.Departman!;
        return new Baglam(bas, bit, ayar, kapsam, kullanicilar, await uow.Sorular.Query.ToDictionaryAsync(s => s.Id, ct), await gq.ToListAsync(ct));
    }

    private static RaporOzet Ozet(int gonderilen, int cevaplayan, List<Satir> satirlar, AyarlarDto ayar)
    {
        var oran = gonderilen == 0 ? 0 : Math.Round(cevaplayan * 100.0 / gonderilen, 1);
        if (satirlar.Count == 0) return new RaporOzet(gonderilen, cevaplayan, oran, null, 0, 0, 0, 0);
        return new RaporOzet(gonderilen, cevaplayan, oran, Math.Round(satirlar.Average(x => x.Skor), 1),
            satirlar.Count(x => x.Skor >= 70), satirlar.Count(x => x.Skor is >= 40 and < 70), satirlar.Count(x => x.Skor < 40),
            satirlar.Count(x => x.S.DusukMu(x.C.Deger, ayar.EmojiEsik, ayar.PuanEsik)));
    }
}
