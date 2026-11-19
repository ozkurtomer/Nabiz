using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

/// <summary>Bir kullanıcıya gönderilen paket; e-postadaki link bu kaydın Token'ını taşır.</summary>
public class Gonderim
{
    public int Id { get; set; }
    public int PartiId { get; set; }
    public GonderimPartisi? Parti { get; set; }
    public int KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }
    public DateTime GonderimZamani { get; set; }
    public string Token { get; set; } = "";
    public GonderimDurumu Durum { get; set; } = GonderimDurumu.Bekliyor;
    /// <summary>E-posta gönderimi başarısız olduysa hata mesajı.</summary>
    public string? Hata { get; set; }
    /// <summary>Çalışan "Tamamla" dediğinde true olur; raporlar yalnızca tamamlananları sayar.</summary>
    public bool Tamamlandi { get; set; }
    public DateTime? TamamlanmaZamani { get; set; }
    /// <summary>Cevaplamayanlara gönderilen otomatik hatırlatma sayısı.</summary>
    public int HatirlatmaSayisi { get; set; }
    public DateTime? SonHatirlatma { get; set; }
    public List<Cevap> Cevaplar { get; set; } = new();
}
