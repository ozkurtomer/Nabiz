namespace Nabiz.Domain.Entities;

/// <summary>Kişi bazlı veriye kim ne zaman baktı / hangi kritik işlemi yaptı (gizlilik denetimi).</summary>
public class DenetimKaydi
{
    public int Id { get; set; }
    public DateTime Zaman { get; set; }
    public int? KullaniciId { get; set; }
    public string KullaniciAd { get; set; } = "";
    public string Eylem { get; set; } = "";
    public string? Detay { get; set; }
}
