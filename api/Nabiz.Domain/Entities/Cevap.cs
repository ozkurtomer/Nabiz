namespace Nabiz.Domain.Entities;

/// <summary>Bir gönderimdeki tek bir sorunun cevabı.</summary>
public class Cevap
{
    public int Id { get; set; }
    public int GonderimId { get; set; }
    public int SoruId { get; set; }
    public Soru? Soru { get; set; }
    public int Deger { get; set; }
    public string? Yorum { get; set; }
    /// <summary>Çalışan yorumunun kimliğiyle ilişkilendirilmemesini istediyse true (İK dahil kimse ismini görmez).</summary>
    public bool YorumAnonim { get; set; }
    public DateTime GuncellemeZamani { get; set; }
}
