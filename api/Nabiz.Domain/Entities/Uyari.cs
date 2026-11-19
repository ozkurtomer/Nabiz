using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

public class Uyari
{
    public int Id { get; set; }
    public int CevapId { get; set; }
    public Cevap? Cevap { get; set; }
    public UyariDurumu Durum { get; set; } = UyariDurumu.Yeni;
    public string? IkNotu { get; set; }
    public DateTime Olusturma { get; set; }
}
