using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

public class Soru
{
    public int Id { get; set; }
    public string MetinTr { get; set; } = "";
    public string MetinEn { get; set; } = "";
    public SoruTipi Tip { get; set; }
    public bool Aktif { get; set; } = true;
    /// <summary>Cevap düşükse çalışana gösterilen takip sorusu (ör. "Seni en çok ne zorladı?"). Boşsa genel metin gösterilir.</summary>
    public string? TakipMetniTr { get; set; }
    public string? TakipMetniEn { get; set; }

    /// <summary>İstenen dilde soru metni; o dilde yazılmadıysa diğer dile düşer.</summary>
    public string Metin(string? dil) => Dil.Sec(dil, MetinTr, MetinEn);
    public string? TakipMetni(string? dil) => Dil.SecOpsiyonel(dil, TakipMetniTr, TakipMetniEn);

    /// <summary>Cevabı soru tipinden bağımsız 0-100 skala skoruna çevirir (yüksek = olumlu).</summary>
    public double Skor(int deger) => Tip switch
    {
        SoruTipi.Emoji => (deger - 1) / 4.0 * 100,
        SoruTipi.Puan => (deger - 1) / 9.0 * 100,
        _ => deger * 100.0,
    };

    /// <summary>Cevap değerinin "düşük" sayılıp sayılmadığını soru tipine göre belirler.</summary>
    public bool DusukMu(int deger, int emojiEsik, int puanEsik) => Tip switch
    {
        SoruTipi.EvetHayir => deger == 0,
        SoruTipi.Puan => deger <= puanEsik,
        _ => deger <= emojiEsik,
    };

    /// <summary>Geçerli cevap değerleri (dağılım grafikleri ve doğrulama için).</summary>
    public IEnumerable<int> Degerler() => Tip switch
    {
        SoruTipi.Emoji => Enumerable.Range(1, 5),
        SoruTipi.Puan => Enumerable.Range(1, 10),
        _ => [0, 1],
    };
}
