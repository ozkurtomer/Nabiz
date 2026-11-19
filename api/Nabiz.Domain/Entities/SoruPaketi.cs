namespace Nabiz.Domain.Entities;

/// <summary>Birlikte gönderilen soru grubu (ör. "Motivasyon Paketi").</summary>
public class SoruPaketi
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public bool Aktif { get; set; } = true;
    public List<PaketSoru> Sorular { get; set; } = new();
}

public class PaketSoru
{
    public int PaketId { get; set; }
    public int SoruId { get; set; }
    public Soru? Soru { get; set; }
    public int Sira { get; set; }
}
