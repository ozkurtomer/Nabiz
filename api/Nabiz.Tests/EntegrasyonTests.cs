using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Application.Services;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;
using Nabiz.Infrastructure.Persistence;

namespace Nabiz.Tests;

/// <summary>Servisleri bellek içi veritabanıyla uçtan uca sınar: gizlilik kuralları ve cevap akışı.</summary>
public class EntegrasyonTests
{
    private class Zaman : IZaman
    {
        public DateTime UtcNow => new(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Simdi => UtcNow;
        public DateTime Yerel(DateTime utc) => utc;
        public DateTime UtcCevir(DateTime yerel) => DateTime.SpecifyKind(yerel, DateTimeKind.Utc);
    }
    private class Ben(int id, Rol rol) : ICurrentUser { public int Id => id; public int? IdVeyaNull => id; public Rol Rol => rol; }
    private class DenetimYok : IDenetimService
    {
        public Task KaydetAsync(string eylem, string? detay, CancellationToken ct) => Task.CompletedTask;
        public Task<List<DenetimDto>> ListeAsync(CancellationToken ct) => Task.FromResult(new List<DenetimDto>());
    }
    private class BildirimYok : IUyariBildirimService
    {
        public int Cagri;
        public Task DusukPuanAsync(Gonderim g, IReadOnlyList<(Soru Soru, Cevap Cevap)> d, CancellationToken ct) { Cagri += d.Count; return Task.CompletedTask; }
    }

    private static (NabizDbContext Db, UnitOfWork Uow) Ortam()
    {
        var db = new NabizDbContext(new DbContextOptionsBuilder<NabizDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Database.EnsureCreated();   // soru + paket seed'i gelir
        return (db, new UnitOfWork(db));
    }

    private static void Veri(NabizDbContext db, int calisanSayisi)
    {
        db.Kullanicilar.Add(new Kullanici { Id = 101, PersonelId = "IK1", Ad = "İK", Email = "ik@x", Rol = Rol.IK });
        var parti = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = new Zaman().UtcNow.AddHours(-2) };
        for (var i = 1; i <= calisanSayisi; i++)
        {
            db.Kullanicilar.Add(new Kullanici { Id = i, PersonelId = "P" + i, Ad = "Kişi" + i, Email = $"k{i}@x", Departman = "Yazılım" });
            db.Gonderimler.Add(new Gonderim
            {
                Parti = parti, KullaniciId = i, Token = "t" + i, Durum = GonderimDurumu.Gonderildi, GonderimZamani = parti.Zaman,
                Tamamlandi = true, TamamlanmaZamani = parti.Zaman,
                Cevaplar = [new Cevap { SoruId = 1, Deger = 2, Yorum = "yorum" + i, YorumAnonim = i == 1, GuncellemeZamani = parti.Zaman }],
            });
        }
        db.SaveChanges();
    }

    private class Ayarlar : IUygulamaAyarlari
    {
        public string PublicUrl => "https://nabiz.test";
        public bool SmtpAyarli => true; public bool IkMailAktif => true; public string? WebhookUrl => null;
        public bool HatirlatmaAktif => true; public int HatirlatmaGun => 2; public int HatirlatmaMaks => 2; public string? MobilAnahtar => "gizli";
    }
    private class MailKutusu : IEmailSender
    {
        public List<(string Kime, string Konu)> Giden = [];
        public Task GonderAsync(string kime, string konu, string html, CancellationToken ct = default) { Giden.Add((kime, konu)); return Task.CompletedTask; }
    }

    [Fact]
    public async Task Hatirlatma_yalnizca_cevaplamayan_gercek_gonderimlere_ve_en_fazla_belirlenen_kez_gider()
    {
        var (db, uow) = Ortam();
        var eski = new Zaman().UtcNow.AddDays(-3);
        db.Kullanicilar.AddRange(new Kullanici { Id = 1, PersonelId = "1", Ad = "Bekleyen", Email = "b@x" }, new Kullanici { Id = 2, PersonelId = "2", Ad = "Cevaplayan", Email = "c@x" }, new Kullanici { Id = 3, PersonelId = "3", Ad = "Demo", Email = "d@x" });
        var gercek = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = eski };
        var demo = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = eski, Demo = true };
        db.Gonderimler.AddRange(
            new Gonderim { Parti = gercek, KullaniciId = 1, Token = "a", Durum = GonderimDurumu.Gonderildi, GonderimZamani = eski },
            new Gonderim { Parti = gercek, KullaniciId = 2, Token = "b", Durum = GonderimDurumu.Gonderildi, GonderimZamani = eski, Tamamlandi = true },
            new Gonderim { Parti = demo, KullaniciId = 3, Token = "c", Durum = GonderimDurumu.Gonderildi, GonderimZamani = eski });
        db.SaveChanges();

        var kutu = new MailKutusu();
        var servis = new GonderimService(uow, kutu, new Zaman(), new Ayarlar(), new Ben(1, Rol.IK), new AyarService(uow), new DenetimYok(), Microsoft.Extensions.Logging.Abstractions.NullLogger<GonderimService>.Instance);

        Assert.Equal(1, await servis.HatirlatmalariGonderAsync(default));
        Assert.Equal("b@x", Assert.Single(kutu.Giden).Kime);
        Assert.Equal(0, await servis.HatirlatmalariGonderAsync(default));       // az önce hatırlatıldı: bekleme süresi dolmadan tekrar yok
        var g = await db.Gonderimler.FirstAsync(x => x.Token == "a");
        Assert.Equal(1, g.HatirlatmaSayisi);
    }

    [Fact]
    public async Task Mobil_giris_yanlis_anahtari_reddeder_dogru_anahtarda_adres_uretir()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.Add(new Kullanici { Id = 1, PersonelId = "1", Ad = "Ayşe", Email = "a@x" }); db.SaveChanges();
        var servis = new AuthService(uow, new SahteSifre(), new SahteToken(), new MailKutusu(), new Ayarlar());
        await Assert.ThrowsAsync<YetkisizHatasi>(() => servis.MobilGirisAsync("a@x", "yanlis", default));
        await Assert.ThrowsAsync<YetkisizHatasi>(() => servis.MobilGirisAsync("a@x", null, default));
        var sonuc = await servis.MobilGirisAsync("a@x", "gizli", default);
        Assert.StartsWith("https://nabiz.test/giris/dogrula?t=", sonuc.Url);
    }
    private class SahteSifre : IPasswordHasher { public string Hashle(string s) => s; public bool Dogrula(string s, string h) => s == h; }
    private class SahteToken : ITokenService
    {
        public string Olustur(Kullanici k) => "oturum";
        public string GirisLinkiTokeniOlustur(Kullanici k) => "giris-" + k.Id;
        public Task<int?> GirisLinkiTokeniDogrulaAsync(string t) => Task.FromResult<int?>(null);
    }

    private static RaporService Rapor(UnitOfWork uow, int kimId, Rol rol) =>
        new(uow, new Ben(kimId, rol), new Zaman(), new AyarService(uow), new DenetimYok());
    private static readonly RaporFiltre Filtre = new(new DateTime(2026, 3, 1), new DateTime(2026, 3, 20), null, null);

    [Fact]
    public async Task IK_isimleri_gorur_ama_anonim_yorumun_sahibini_goremez()
    {
        var (db, uow) = Ortam(); Veri(db, 4);
        var liste = await Rapor(uow, 101, Rol.IK).CevaplarAsync(Filtre, false, default);
        Assert.Equal(3, liste.Count(c => c.Kisi is not null));
        var anonim = Assert.Single(liste, c => c.Kisi is null);
        Assert.Equal("yorum1", anonim.Yorum);
    }

    [Fact]
    public async Task IK_raporu_tum_calisanlari_ve_departmanlari_kapsar()
    {
        var (db, uow) = Ortam(); Veri(db, 3);
        var rapor = await Rapor(uow, 101, Rol.IK).GetirAsync(Filtre, default);
        Assert.Equal("Tüm şirket", rapor.Kapsam);
        Assert.Equal(3, rapor.Ozet.Gonderilen);
        Assert.Equal(3, rapor.Ozet.Cevaplayan);
        Assert.Single(rapor.Departmanlar);
        Assert.Equal(3, rapor.Ozet.DusukPuan);         // emoji 2 ≤ eşik 2
    }

    [Fact]
    public async Task Calisan_rapor_goremez()
    {
        var (db, uow) = Ortam(); Veri(db, 4);
        db.Kullanicilar.Add(new Kullanici { Id = 200, PersonelId = "C", Ad = "Çalışan", Email = "c@x", Rol = Rol.Calisan }); db.SaveChanges();
        await Assert.ThrowsAsync<YetkisizHatasi>(() => Rapor(uow, 200, Rol.Calisan).GetirAsync(Filtre, default));
    }

    [Fact]
    public async Task Kisi_gecmisi_yalnizca_IK_icin()
    {
        var (db, uow) = Ortam(); Veri(db, 4);
        db.Kullanicilar.Add(new Kullanici { Id = 200, PersonelId = "C", Ad = "Çalışan", Email = "c@x", Rol = Rol.Calisan }); db.SaveChanges();
        await Assert.ThrowsAsync<YetkisizHatasi>(() => Rapor(uow, 200, Rol.Calisan).KisiGecmisiAsync(2, default));
        var g = await Rapor(uow, 101, Rol.IK).KisiGecmisiAsync(2, default);
        Assert.Single(g.Noktalar);
    }

    [Fact]
    public async Task Cevap_akisi_eksik_tamamlamayi_reddeder_ve_dusuk_puandan_uyari_uretir()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.Add(new Kullanici { Id = 1, PersonelId = "P", Ad = "Ayşe", Email = "a@x" });
        db.Gonderimler.Add(new Gonderim { Parti = new GonderimPartisi { PaketId = 3, HedefTipi = HedefTipi.Hepsi, Zaman = DateTime.UtcNow }, KullaniciId = 1, Token = "abc", Durum = GonderimDurumu.Gonderildi, GonderimZamani = DateTime.UtcNow });
        db.SaveChanges();
        var bildirim = new BildirimYok();
        var servis = new CevapService(uow, new AyarService(uow), new Zaman(), bildirim);

        // Paket 3 = sorular 4 (Evet/Hayır), 8 (Emoji), 10 (Puan)
        await Assert.ThrowsAsync<DogrulamaHatasi>(() => servis.KaydetAsync("abc", new CevapKaydetIstek([new(4, 1, null)], true), default));
        await Assert.ThrowsAsync<DogrulamaHatasi>(() => servis.KaydetAsync("abc", new CevapKaydetIstek([new(8, 9, null)], false), default));   // emoji için geçersiz değer

        var yanit = await servis.KaydetAsync("abc", new CevapKaydetIstek([new(4, 0, "yoğun", true), new(8, 5, null), new(10, 9, null)], true), default);
        Assert.True(yanit.Tamamlandi);
        Assert.Equal(1, await db.Uyarilar.CountAsync());        // yalnızca Hayır (0) düşük
        Assert.Equal(1, bildirim.Cagri);
        Assert.True((await db.Cevaplar.FirstAsync(c => c.SoruId == 4)).YorumAnonim);
        await Assert.ThrowsAsync<CakismaHatasi>(() => servis.KaydetAsync("abc", new CevapKaydetIstek([new(4, 1, null)], false), default));  // tamamlanan değişmez
    }

    [Fact]
    public async Task Kullanici_personel_senkronu_olmayanlari_pasiflestirir()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.AddRange(new Kullanici { Id = 1, PersonelId = "A", Ad = "A", Email = "a@x" }, new Kullanici { Id = 2, PersonelId = "B", Ad = "B", Email = "b@x" },
            new Kullanici { Id = 3, PersonelId = Kullanici.IlkPersonelId, Ad = "İlk İK", Email = "ad@x", Rol = Rol.IK });
        db.SaveChanges();
        var kaynak = new SabitPersonel([new PersonelKaydi("A", "A Yeni", "a@x", "BT"), new PersonelKaydi("C", "C", "c@x", "BT")]);
        var servis = new KullaniciService(uow, kaynak, new DenetimYok(), new Zaman());

        var sonuc = await servis.PersonelAktarAsync(default);
        Assert.Equal((1, 1), (sonuc.Yeni, sonuc.Guncel));
        Assert.False((await db.Kullanicilar.FindAsync(2))!.Aktif);      // kaynakta yok → pasif
        Assert.True((await db.Kullanicilar.FindAsync(3))!.Aktif);       // ilk İK hesabı korunur
        Assert.Equal("A Yeni", (await db.Kullanicilar.FindAsync(1))!.Ad);
    }

    private class SabitPersonel(PersonelKaydi[] kayitlar) : IPersonelKaynagi
    {
        public Task<IReadOnlyList<PersonelKaydi>> OkuAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<PersonelKaydi>>(kayitlar);
    }
}
