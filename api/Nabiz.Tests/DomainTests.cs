using Nabiz.Domain;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Tests;

public class DomainTests
{
    [Theory]
    [InlineData(SoruTipi.Emoji, 1, 0)]
    [InlineData(SoruTipi.Emoji, 3, 50)]
    [InlineData(SoruTipi.Emoji, 5, 100)]
    [InlineData(SoruTipi.Puan, 1, 0)]
    [InlineData(SoruTipi.Puan, 10, 100)]
    [InlineData(SoruTipi.EvetHayir, 0, 0)]
    [InlineData(SoruTipi.EvetHayir, 1, 100)]
    public void Skor_tum_tipleri_0_100_arasina_normalize_eder(SoruTipi tip, int deger, double beklenen) =>
        Assert.Equal(beklenen, new Soru { Tip = tip }.Skor(deger), 1);

    [Theory]
    [InlineData(SoruTipi.Emoji, 2, true)]
    [InlineData(SoruTipi.Emoji, 3, false)]
    [InlineData(SoruTipi.Puan, 4, true)]
    [InlineData(SoruTipi.Puan, 5, false)]
    [InlineData(SoruTipi.EvetHayir, 0, true)]
    [InlineData(SoruTipi.EvetHayir, 1, false)]
    public void DusukMu_esikleri_soru_tipine_gore_uygular(SoruTipi tip, int deger, bool beklenen) =>
        Assert.Equal(beklenen, new Soru { Tip = tip }.DusukMu(deger, emojiEsik: 2, puanEsik: 4));

    [Fact]
    public void Degerler_tipe_gore_gecerli_degerleri_verir()
    {
        Assert.Equal(5, new Soru { Tip = SoruTipi.Emoji }.Degerler().Count());
        Assert.Equal(10, new Soru { Tip = SoruTipi.Puan }.Degerler().Count());
        Assert.Equal([0, 1], new Soru { Tip = SoruTipi.EvetHayir }.Degerler());
    }

    [Theory]
    [InlineData("3,7,12", new[] { 3, 7, 12 })]
    [InlineData(" 3 , x, 3, 0, 5 ", new[] { 3, 5 })]
    [InlineData("", new int[0])]
    [InlineData(null, new int[0])]
    public void KisiIdleri_gecersiz_ve_tekrarlari_eler(string? girdi, int[] beklenen) =>
        Assert.Equal(beklenen, Hedef.KisiIdleri(girdi));
}
