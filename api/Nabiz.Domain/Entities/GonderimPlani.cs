using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

public class GonderimPlani
{
    public int Id { get; set; }
    public int PaketId { get; set; }
    public SoruPaketi? Paket { get; set; }
    public HedefTipi HedefTipi { get; set; }
    /// <summary>Departman adı (Departman) veya virgülle ayrılmış kullanıcı Id'leri (Kisi).</summary>
    public string? HedefDeger { get; set; }
    public DateTime Baslangic { get; set; }
    /// <summary>Kaç günde bir gönderilir (1 = her gün).</summary>
    public int SiklikGun { get; set; } = 1;
    public TimeSpan Saat { get; set; } = new(9, 0, 0);
    public DateTime? SonCalisma { get; set; }
    public bool Aktif { get; set; } = true;
}
