using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;

namespace Nabiz.Application.Services;

public interface IKvkkService
{
    Task<KvkkAyarDto> AyarGetirAsync(CancellationToken ct);
    Task AyarKaydetAsync(KvkkAyarDto ayar, CancellationToken ct);
    /// <summary>Çalışanın cevap ekranında gördüğü aydınlatma metni (iki dil).</summary>
    Task<(string Tr, string En)> AydinlatmaAsync(CancellationToken ct);
    Task<SaklamaOnizleme> SaklamaOnizleAsync(CancellationToken ct);
    Task<SaklamaSonucu> SaklamaUygulaAsync(CancellationToken ct);
    /// <summary>Zamanlayıcı: günde bir kez, saklama süresi dolan verileri siler.</summary>
    Task<SaklamaSonucu?> SaklamaGunlukAsync(CancellationToken ct);
    /// <summary>Çalışanın verisini (yorumu anonim olanlar hariç) CSV olarak dışa aktarır: erişim hakkı.</summary>
    Task<byte[]> KisiDisaAktarAsync(int kullaniciId, CancellationToken ct);
    /// <summary>Çalışanın cevaplarını, uyarılarını ve aksiyonlarını kalıcı siler: silinme hakkı.</summary>
    Task<KisiSilSonucu> KisiSilAsync(int kullaniciId, CancellationToken ct);
}

public class KvkkService(IUnitOfWork uow, IAyarDeposu depo, IZaman zaman, IDenetimService denetim, IDilBilgisi? dilBilgisi = null) : IKvkkService
{
    public const int VarsayilanSaklamaAy = 24;

    public const string VarsayilanAydinlatmaTr =
        "Nabız, çalışan esenliğini ölçmek amacıyla verdiğin cevapları ve (varsa) yorumlarını işler. " +
        "Cevapların yalnızca yetkili İnsan Kaynakları ekibi tarafından görülür; yorumunu anonim bırakırsan kimse onu seninle ilişkilendiremez. " +
        "Veriler şirketin belirlediği saklama süresi boyunca tutulur ve süre sonunda silinir. " +
        "Verilerine erişme, düzeltilmesini ya da silinmesini isteme haklarını kullanmak için İnsan Kaynakları ile iletişime geçebilirsin.";

    public const string VarsayilanAydinlatmaEn =
        "Nabız processes the answers and comments you provide in order to measure employee wellbeing. " +
        "Your answers are visible only to the authorized Human Resources team; if you leave a comment anonymous, nobody can link it to you. " +
        "Data is kept for the retention period set by your company and deleted afterwards. " +
        "To exercise your rights to access, correct or delete your data, please contact Human Resources.";

    public async Task<KvkkAyarDto> AyarGetirAsync(CancellationToken ct)
    {
        var ay = int.TryParse(await depo.OkuAsync(AyarAnahtarlari.SaklamaAy, ct), out var n) ? n : VarsayilanSaklamaAy;
        var tr = await depo.OkuAsync(AyarAnahtarlari.AydinlatmaTr, ct);
        var en = await depo.OkuAsync(AyarAnahtarlari.AydinlatmaEn, ct);
        return new KvkkAyarDto(ay, string.IsNullOrWhiteSpace(tr) ? VarsayilanAydinlatmaTr : tr, string.IsNullOrWhiteSpace(en) ? VarsayilanAydinlatmaEn : en);
    }

    public async Task AyarKaydetAsync(KvkkAyarDto a, CancellationToken ct)
    {
        if (a.SaklamaAy is < 0 or > 120) throw new DogrulamaHatasi("Saklama süresi 0-120 ay olmalı (0 = süresiz).");
        if ((a.AydinlatmaTr?.Length ?? 0) > 4000 || (a.AydinlatmaEn?.Length ?? 0) > 4000) throw new DogrulamaHatasi("Aydınlatma metni en fazla 4000 karakter olabilir.");
        await depo.YazAsync(AyarAnahtarlari.SaklamaAy, a.SaklamaAy.ToString(), ct);
        await depo.YazAsync(AyarAnahtarlari.AydinlatmaTr, (a.AydinlatmaTr ?? "").Trim(), ct);
        await depo.YazAsync(AyarAnahtarlari.AydinlatmaEn, (a.AydinlatmaEn ?? "").Trim(), ct);
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("KVKK ayarları değişti", $"saklama: {a.SaklamaAy}", ct);
    }

    public async Task<(string Tr, string En)> AydinlatmaAsync(CancellationToken ct)
    {
        var a = await AyarGetirAsync(ct);
        return (a.AydinlatmaTr, a.AydinlatmaEn);
    }

    public async Task<SaklamaOnizleme> SaklamaOnizleAsync(CancellationToken ct)
    {
        var ay = (await AyarGetirAsync(ct)).SaklamaAy;
        if (ay == 0) return new SaklamaOnizleme(0, null, 0, 0);
        var kesim = zaman.UtcNow.AddMonths(-ay);
        var g = uow.Gonderimler.Query.Where(x => x.GonderimZamani < kesim);
        return new SaklamaOnizleme(ay, kesim, await g.CountAsync(ct), await g.SelectMany(x => x.Cevaplar).CountAsync(ct));
    }

    public async Task<SaklamaSonucu> SaklamaUygulaAsync(CancellationToken ct)
    {
        var sonuc = await SaklamaCalistirAsync(ct) ?? new SaklamaSonucu(0, 0);
        await denetim.KaydetAsync("Saklama süresi temizliği", $"{sonuc.Gonderim}/{sonuc.Cevap}", ct);
        return sonuc;
    }

    public async Task<SaklamaSonucu?> SaklamaGunlukAsync(CancellationToken ct)
    {
        var bugun = zaman.Simdi.ToString("yyyy-MM-dd");
        if (await depo.OkuAsync(AyarAnahtarlari.SonSaklamaTemizligi, ct) == bugun) return null;
        var sonuc = await SaklamaCalistirAsync(ct);
        await depo.YazAsync(AyarAnahtarlari.SonSaklamaTemizligi, bugun, ct);
        await uow.SaveChangesAsync(ct);
        if (sonuc is { Gonderim: > 0 }) await denetim.KaydetAsync("Saklama süresi temizliği (otomatik)", $"{sonuc.Gonderim}/{sonuc.Cevap}", ct);
        return sonuc;
    }

    private async Task<SaklamaSonucu?> SaklamaCalistirAsync(CancellationToken ct)
    {
        var ay = (await AyarGetirAsync(ct)).SaklamaAy;
        if (ay == 0) return null;
        var kesim = zaman.UtcNow.AddMonths(-ay);
        var eski = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Where(g => g.GonderimZamani < kesim).ToListAsync(ct);
        var cevapIds = eski.SelectMany(g => g.Cevaplar).Select(c => c.Id).ToList();
        var uyarilar = await uow.Uyarilar.Query.Where(u => cevapIds.Contains(u.CevapId)).ToListAsync(ct);
        uow.Uyarilar.RemoveRange(uyarilar);
        uow.Gonderimler.RemoveRange(eski);
        // Boşalan eski gönderim partilerini de temizle
        var bosPartiler = await uow.GonderimPartileri.Query.Where(p => p.Zaman < kesim && !uow.Gonderimler.Query.Any(g => g.PartiId == p.Id && !eski.Select(e => e.Id).Contains(g.Id))).ToListAsync(ct);
        uow.GonderimPartileri.RemoveRange(bosPartiler);
        await uow.SaveChangesAsync(ct);
        return new SaklamaSonucu(eski.Count, cevapIds.Count);
    }

    public async Task<byte[]> KisiDisaAktarAsync(int kullaniciId, CancellationToken ct)
    {
        var dil = dilBilgisi?.Dil ?? "tr";
        var k = await uow.Kullanicilar.FindAsync(kullaniciId, ct) ?? throw new BulunamadiHatasi("Kullanıcı bulunamadı.");
        var sorular = await uow.Sorular.Query.ToDictionaryAsync(s => s.Id, ct);
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Include(g => g.Parti!).ThenInclude(p => p.Paket)
            .Where(g => g.KullaniciId == kullaniciId).OrderBy(g => g.GonderimZamani).ToListAsync(ct);

        var satirlar = new List<object?[]>
        {
            new object?[] { "Ad", "E-posta", "Departman", "Dil" },
            new object?[] { k.Ad, k.Email, k.Departman, k.Dil },
            new object?[] { },
            new object?[] { "Tarih", "Paket", "Soru", "Cevap", "Yorum", "Tamamlandı" },
        };
        foreach (var g in gonderimler)
            foreach (var c in g.Cevaplar)
                satirlar.Add([zaman.Yerel(g.GonderimZamani), g.Parti!.Paket!.AdGoster(dil), sorular[c.SoruId].Metin(dil), c.Deger,
                    c.YorumAnonim ? "(anonim: ilişkilendirilemez)" : c.Yorum, g.Tamamlandi]);
        await denetim.KaydetAsync("Kişi verisi dışa aktarıldı", k.Ad, ct);
        return Csv.Uret(satirlar);
    }

    public async Task<KisiSilSonucu> KisiSilAsync(int kullaniciId, CancellationToken ct)
    {
        var k = await uow.Kullanicilar.FindAsync(kullaniciId, ct) ?? throw new BulunamadiHatasi("Kullanıcı bulunamadı.");
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Where(g => g.KullaniciId == kullaniciId).ToListAsync(ct);
        var cevapIds = gonderimler.SelectMany(g => g.Cevaplar).Select(c => c.Id).ToList();
        var uyarilar = await uow.Uyarilar.Query.Where(u => cevapIds.Contains(u.CevapId)).ToListAsync(ct);
        var aksiyonlar = await uow.Aksiyonlar.Query.Where(a => a.KullaniciId == kullaniciId).ToListAsync(ct);
        uow.Uyarilar.RemoveRange(uyarilar);
        uow.Aksiyonlar.RemoveRange(aksiyonlar);
        uow.Gonderimler.RemoveRange(gonderimler);
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("Kişi verisi silindi (KVKK)", k.Ad, ct);
        return new KisiSilSonucu(gonderimler.Count, cevapIds.Count, uyarilar.Count, aksiyonlar.Count);
    }
}
