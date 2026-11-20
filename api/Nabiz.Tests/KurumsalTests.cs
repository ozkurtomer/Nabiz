using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Application.Services;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;
using Nabiz.Infrastructure.Persistence;

namespace Nabiz.Tests;

/// <summary>Marka, aksiyon takibi ve KVKK (saklama, dışa aktarma, silme) servislerini bellek içi veritabanıyla sınar.</summary>
public class KurumsalTests
{
    private class Zaman : IZaman
    {
        public DateTime UtcNow => new(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Simdi => UtcNow;
        public DateTime Yerel(DateTime utc) => utc;
        public DateTime UtcCevir(DateTime yerel) => DateTime.SpecifyKind(yerel, DateTimeKind.Utc);
    }
    private class Ben : ICurrentUser { public int Id => 101; public int? IdVeyaNull => 101; public Rol Rol => Rol.IK; }
    private class DenetimYok : IDenetimService
    {
        public List<string> Eylemler = [];
        public Task KaydetAsync(string eylem, string? detay, CancellationToken ct) { Eylemler.Add(eylem); return Task.CompletedTask; }
        public Task<List<DenetimDto>> ListeAsync(CancellationToken ct) => Task.FromResult(new List<DenetimDto>());
    }

    private static (NabizDbContext Db, UnitOfWork Uow) Ortam()
    {
        var db = new NabizDbContext(new DbContextOptionsBuilder<NabizDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Database.EnsureCreated();
        db.Kullanicilar.Add(new Kullanici { Id = 101, PersonelId = "IK", Ad = "İK", Email = "ik@x", Rol = Rol.IK });
        db.SaveChanges();
        return (db, new UnitOfWork(db));
    }

    [Fact]
    public async Task Marka_varsayilani_doner_ve_gecersiz_rengi_reddeder()
    {
        var (_, uow) = Ortam();
        var s = new MarkaService(uow, new AyarDeposu(uow), new DenetimYok());
        Assert.Equal("Nabız", (await s.GetirAsync(default)).Ad);
        await Assert.ThrowsAsync<DogrulamaHatasi>(() => s.KaydetAsync(new MarkaDto("Acme", "kirmizi"), default));
        await s.KaydetAsync(new MarkaDto("Acme", "#AA00FF"), default);
        var m = await s.GetirAsync(default);
        Assert.Equal(("Acme", "#aa00ff"), (m.Ad, m.Renk));
    }

    [Fact]
    public async Task Aksiyon_tamamlaninca_kapanma_damgalanir_ve_gecikme_hesaplanir()
    {
        var (_, uow) = Ortam();
        var s = new AksiyonService(uow, new Zaman(), new Ben(), new DenetimYok());
        var a = await s.EkleAsync(new AksiyonKaydetIstek("Ofis ısısı", null, AksiyonDurumu.Acik, AksiyonOnceligi.Yuksek, 101, new DateTime(2026, 3, 1), null, null, null), default);
        Assert.True(a.Gecikmis);
        Assert.Equal(1, (await s.OzetAsync(default)).Gecikmis);
        var k = await s.GuncelleAsync(a.Id, new AksiyonKaydetIstek("Ofis ısısı", null, AksiyonDurumu.Tamam, AksiyonOnceligi.Yuksek, 101, new DateTime(2026, 3, 1), null, null, "çözüldü"), default);
        Assert.NotNull(k.Kapanma);
        Assert.False(k.Gecikmis);
        await Assert.ThrowsAsync<DogrulamaHatasi>(() => s.EkleAsync(new AksiyonKaydetIstek(" ", null, AksiyonDurumu.Acik, AksiyonOnceligi.Orta, null, null, null, null, null), default));
    }

    [Fact]
    public async Task Aksiyon_uyariden_olusturulunca_uyari_takibe_alinir()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.Add(new Kullanici { Id = 1, PersonelId = "1", Ad = "A", Email = "a@x" });
        var parti = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = new Zaman().UtcNow };
        var g = new Gonderim { Parti = parti, KullaniciId = 1, Token = "t", Durum = GonderimDurumu.Gonderildi, GonderimZamani = parti.Zaman, Tamamlandi = true, Cevaplar = [new Cevap { SoruId = 1, Deger = 1 }] };
        db.Gonderimler.Add(g); db.SaveChanges();
        var uyari = new Uyari { CevapId = g.Cevaplar[0].Id, Durum = UyariDurumu.Yeni };
        db.Uyarilar.Add(uyari); db.SaveChanges();
        var s = new AksiyonService(uow, new Zaman(), new Ben(), new DenetimYok());
        await s.EkleAsync(new AksiyonKaydetIstek("Görüş", null, AksiyonDurumu.Acik, AksiyonOnceligi.Orta, 101, null, uyari.Id, 1, null), default);
        Assert.Equal(UyariDurumu.Takipte, db.Uyarilar.First().Durum);
    }

    [Fact]
    public async Task Saklama_suresi_dolan_gonderimleri_siler_yenileri_korur()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.Add(new Kullanici { Id = 1, PersonelId = "1", Ad = "A", Email = "a@x" });
        var eski = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = new Zaman().UtcNow.AddMonths(-30) };
        var yeni = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = new Zaman().UtcNow.AddDays(-5) };
        db.Gonderimler.AddRange(
            new Gonderim { Parti = eski, KullaniciId = 1, Token = "e", Durum = GonderimDurumu.Gonderildi, GonderimZamani = eski.Zaman, Tamamlandi = true, Cevaplar = [new Cevap { SoruId = 1, Deger = 3 }] },
            new Gonderim { Parti = yeni, KullaniciId = 1, Token = "y", Durum = GonderimDurumu.Gonderildi, GonderimZamani = yeni.Zaman, Tamamlandi = true, Cevaplar = [new Cevap { SoruId = 1, Deger = 4 }] });
        db.SaveChanges();
        var k = new KvkkService(uow, new AyarDeposu(uow), new Zaman(), new DenetimYok());
        var onizle = await k.SaklamaOnizleAsync(default);
        Assert.Equal(1, onizle.Gonderim);
        var sonuc = await k.SaklamaUygulaAsync(default);
        Assert.Equal(1, sonuc.Gonderim);
        Assert.Equal(["y"], db.Gonderimler.Select(x => x.Token).ToList());
    }

    [Fact]
    public async Task Saklama_sifirsa_hicbir_sey_silinmez()
    {
        var (db, uow) = Ortam();
        var k = new KvkkService(uow, new AyarDeposu(uow), new Zaman(), new DenetimYok());
        await k.AyarKaydetAsync(new KvkkAyarDto(0, "tr", "en"), default);
        Assert.Equal(0, (await k.SaklamaOnizleAsync(default)).Gonderim);
        await Assert.ThrowsAsync<DogrulamaHatasi>(() => k.AyarKaydetAsync(new KvkkAyarDto(999, "", ""), default));
    }

    [Fact]
    public async Task Kisi_disa_aktarimi_anonim_yorumu_icermez_ve_silme_her_seyi_temizler()
    {
        var (db, uow) = Ortam();
        db.Kullanicilar.Add(new Kullanici { Id = 1, PersonelId = "1", Ad = "Ayşe", Email = "a@x" });
        var parti = new GonderimPartisi { PaketId = 1, HedefTipi = HedefTipi.Hepsi, Zaman = new Zaman().UtcNow };
        db.Gonderimler.Add(new Gonderim { Parti = parti, KullaniciId = 1, Token = "t", Durum = GonderimDurumu.Gonderildi, GonderimZamani = parti.Zaman, Tamamlandi = true,
            Cevaplar = [new Cevap { SoruId = 1, Deger = 2, Yorum = "gizli sır", YorumAnonim = true }, new Cevap { SoruId = 2, Deger = 8, Yorum = "açık yorum" }] });
        db.SaveChanges();
        var k = new KvkkService(uow, new AyarDeposu(uow), new Zaman(), new DenetimYok());
        var csv = System.Text.Encoding.UTF8.GetString((await k.KisiDisaAktarAsync(1, default)).Skip(3).ToArray());
        Assert.DoesNotContain("gizli sır", csv);
        Assert.Contains("açık yorum", csv);
        var r = await k.KisiSilAsync(1, default);
        Assert.Equal(1, r.Gonderim);
        Assert.Equal(2, r.Cevap);
        Assert.Empty(db.Gonderimler);
        Assert.Empty(db.Cevaplar);
    }

    private class DuzSifre : IPasswordHasher { public string Hashle(string s) => s; public bool Dogrula(string s, string h) => s == h; }
    private class TokenYok : ITokenService
    {
        public string Olustur(Kullanici k) => "oturum";
        public string GirisLinkiTokeniOlustur(Kullanici k) => "giris";
        public Task<int?> GirisLinkiTokeniDogrulaAsync(string t) => Task.FromResult<int?>(null);
    }

    [Fact]
    public async Task Giris_bes_hatali_denemeden_sonra_kilitlenir_ve_dogru_sifre_bile_reddedilir()
    {
        var (db, uow) = Ortam();
        var email = $"kilit-{Guid.NewGuid():N}@x";
        db.Kullanicilar.Add(new Kullanici { Id = 150, PersonelId = "K", Ad = "Kilit", Email = email, Rol = Rol.IK, SifreHash = "dogru" });
        db.SaveChanges();
        var auth = new AuthService(uow, new DuzSifre(), new TokenYok(), null!, null!);
        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<YetkisizHatasi>(() => auth.LoginAsync(new LoginIstek(email, "yanlis"), default));
        var ex = await Assert.ThrowsAsync<YetkisizHatasi>(() => auth.LoginAsync(new LoginIstek(email, "dogru"), default));
        Assert.Contains("Çok fazla", ex.Message);
    }

    [Fact]
    public async Task Basarili_giris_hatali_deneme_sayacini_sifirlar()
    {
        var (db, uow) = Ortam();
        var email = $"sifirla-{Guid.NewGuid():N}@x";
        db.Kullanicilar.Add(new Kullanici { Id = 151, PersonelId = "S", Ad = "Sifir", Email = email, Rol = Rol.IK, SifreHash = "dogru" });
        db.SaveChanges();
        var auth = new AuthService(uow, new DuzSifre(), new TokenYok(), null!, null!);
        for (var i = 0; i < 4; i++)
            await Assert.ThrowsAsync<YetkisizHatasi>(() => auth.LoginAsync(new LoginIstek(email, "yanlis"), default));
        Assert.NotNull(await auth.LoginAsync(new LoginIstek(email, "dogru"), default));
        for (var i = 0; i < 4; i++)
            await Assert.ThrowsAsync<YetkisizHatasi>(() => auth.LoginAsync(new LoginIstek(email, "yanlis"), default));
        Assert.NotNull(await auth.LoginAsync(new LoginIstek(email, "dogru"), default));
    }
}
