using Nabiz.Application.Services;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Tests;

public class ApplicationTests
{
    private static GonderimPlani Plan(string baslangic, string saat, int siklik, string? sonCalisma = null) => new()
    {
        Baslangic = DateTime.Parse(baslangic), Saat = TimeSpan.Parse(saat), SiklikGun = siklik,
        SonCalisma = sonCalisma is null ? null : DateTime.Parse(sonCalisma),
    };

    [Fact] public void Plan_baslangic_gelmediyse_calismaz() =>
        Assert.False(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 1), DateTime.Parse("2026-03-09 12:00")));

    [Fact] public void Plan_saat_gelmediyse_calismaz() =>
        Assert.False(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 1), DateTime.Parse("2026-03-10 08:59")));

    [Fact] public void Plan_ilk_kez_vaktinde_calisir() =>
        Assert.True(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 1), DateTime.Parse("2026-03-10 09:00")));

    [Fact] public void Plan_ayni_gun_ikinci_kez_calismaz() =>
        Assert.False(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 1, "2026-03-10 09:00"), DateTime.Parse("2026-03-10 15:00")));

    [Theory]
    [InlineData(1, "2026-03-11 09:00", true)]
    [InlineData(7, "2026-03-16 09:00", false)]
    [InlineData(7, "2026-03-17 09:00", true)]
    public void Plan_siklik_gune_gore_calisir(int siklik, string simdi, bool beklenen) =>
        Assert.Equal(beklenen, GonderimService.Vadesi(Plan("2026-03-01", "09:00", siklik, "2026-03-10 09:00"), DateTime.Parse(simdi)));

    [Fact] public void Bir_kez_plani_ilk_vaktinde_calisir() =>
        Assert.True(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 0), DateTime.Parse("2026-03-10 09:30")));

    [Fact] public void Bir_kez_plani_calistiktan_sonra_bir_daha_calismaz() =>
        Assert.False(GonderimService.Vadesi(Plan("2026-03-10", "09:00", 0, "2026-03-10 09:30"), DateTime.Parse("2026-04-20 12:00")));

    [Fact]
    public void Csv_bom_noktalivirgul_ve_kacis_kurallarini_uygular()
    {
        var b = Csv.Uret([["Ad", "Not"], ["Ayşe", "a;b \"c\""], ["Can", true]]);
        Assert.Equal([0xEF, 0xBB, 0xBF], b.Take(3).ToArray());
        var metin = System.Text.Encoding.UTF8.GetString(b.Skip(3).ToArray());
        Assert.Contains("Ad;Not", metin);
        Assert.Contains("\"a;b \"\"c\"\"\"", metin);
        Assert.Contains("Can;Evet", metin);
    }

    [Fact]
    public void Csv_formul_enjeksiyonunu_engeller()
    {
        var metin = System.Text.Encoding.UTF8.GetString(Csv.Uret([["=1+1", "@x", "-5"]]).Skip(3).ToArray());
        Assert.StartsWith("'=1+1;'@x;-5", metin);   // sayılar korunur, formüller metne çevrilir
    }

    [Fact]
    public void Mail_sablonu_linki_ve_dili_icerir()
    {
        var paket = new SoruPaketi { Ad = "Motivasyon", Sorular = [new PaketSoru { Sira = 0, Soru = new Soru { MetinTr = "Bir", MetinEn = "One", Tip = SoruTipi.Emoji } }, new PaketSoru { Sira = 1, Soru = new Soru { MetinTr = "İki", MetinEn = "Two", Tip = SoruTipi.Puan } }] };
        var (konuTr, htmlTr) = EmailSablonu.Paket(new Kullanici { Ad = "Ayşe Yılmaz", Dil = "tr" }, paket, "https://x/cevap/abc");
        var (konuEn, htmlEn) = EmailSablonu.Paket(new Kullanici { Ad = "Ayşe Yılmaz", Dil = "en" }, paket, "https://x/cevap/abc");
        Assert.Contains("https://x/cevap/abc", htmlTr);
        Assert.Contains("Merhaba Ayşe", htmlTr);
        Assert.Contains("2 kısa soru", htmlTr);
        Assert.Contains("Hi Ayşe", htmlEn);
        Assert.NotEqual(konuTr, konuEn);
    }

    [Fact]
    public void Mail_sablonu_html_enjeksiyonunu_kodlar()
    {
        var paket = new SoruPaketi { Ad = "<script>x</script>", Sorular = [new PaketSoru { Soru = new Soru { MetinTr = "a", MetinEn = "a" } }, new PaketSoru { Soru = new Soru { MetinTr = "b", MetinEn = "b" } }] };
        var (_, html) = EmailSablonu.Paket(new Kullanici { Ad = "<b>Ali</b>", Dil = "tr" }, paket, "https://x/c/1");
        Assert.DoesNotContain("<script>x</script>", html);
        Assert.DoesNotContain("<b>Ali</b>", html);
    }
}
