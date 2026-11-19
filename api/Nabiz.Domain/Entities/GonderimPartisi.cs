using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

/// <summary>Tek seferlik bir gönderim turu (plan çalışması ya da anlık gönderim). Raporlar parti bazında listelenir.</summary>
public class GonderimPartisi
{
    public int Id { get; set; }
    public int PaketId { get; set; }
    public SoruPaketi? Paket { get; set; }
    public HedefTipi HedefTipi { get; set; }
    public string? HedefDeger { get; set; }
    public DateTime Zaman { get; set; }
    public int? PlanId { get; set; }
    /// <summary>true: İK elle anlık gönderdi, false: plan çalıştırdı.</summary>
    public bool Anlik { get; set; }
    /// <summary>Deneme amaçlı üretilmiş sahte veri (silinebilir).</summary>
    public bool Demo { get; set; }
    /// <summary>İK kendine denemek için ürettiği, e-posta atılmayan gönderim.</summary>
    public bool Test { get; set; }
    public int? OlusturanKullaniciId { get; set; }
}
