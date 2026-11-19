namespace Nabiz.Domain.Entities;

/// <summary>Birlikte gönderilen soru grubu (ör. "Motivasyon Paketi").</summary>
public class SoruPaketi
{
    public int Id { get; set; }
    /// <summary>Türkçe ad. Türkçe ve İngilizce adlardan en az biri dolu olmalıdır.</summary>
    public string Ad { get; set; } = "";
    public string AdEn { get; set; } = "";
    public string? Aciklama { get; set; }
    public string? AciklamaEn { get; set; }
    public bool Aktif { get; set; } = true;

    public string AdGoster(string? dil) => Dil.Sec(dil, Ad, AdEn);
    public string? AciklamaGoster(string? dil) => Dil.SecOpsiyonel(dil, Aciklama, AciklamaEn);
    public List<PaketSoru> Sorular { get; set; } = new();
}

public class PaketSoru
{
    public int PaketId { get; set; }
    public int SoruId { get; set; }
    public Soru? Soru { get; set; }
    public int Sira { get; set; }
}
