using Nabiz.Domain.Entities;

namespace Nabiz.Application.Abstractions;

public interface IPasswordHasher
{
    string Hashle(string sifre);
    bool Dogrula(string sifre, string hash);
}

public interface ITokenService
{
    string Olustur(Kullanici kullanici);
    /// <summary>E-postayla gönderilen, kısa ömürlü, tek amaçlı giriş token'ı.</summary>
    string GirisLinkiTokeniOlustur(Kullanici kullanici);
    /// <summary>Geçerliyse kullanıcı Id'sini döner.</summary>
    Task<int?> GirisLinkiTokeniDogrulaAsync(string token);
}

public interface ICurrentUser
{
    int Id { get; }
    /// <summary>Oturum yoksa (ör. zamanlayıcı) null.</summary>
    int? IdVeyaNull { get; }
    Nabiz.Domain.Enums.Rol Rol { get; }
}

public interface IEmailSender
{
    Task GonderAsync(string kime, string konu, string htmlGovde, CancellationToken ct = default);
}

/// <summary>Şirket saat dilimine göre zaman; sunucu UTC olsa bile plan saatleri yerel saatle çalışır.</summary>
public interface IZaman
{
    DateTime UtcNow { get; }
    DateTime Simdi { get; }
    DateTime Yerel(DateTime utc);
    DateTime UtcCevir(DateTime yerel);
}

public interface IUygulamaAyarlari
{
    /// <summary>E-postadaki linklerin kök adresi (ör. https://nabiz.sirket.com).</summary>
    string PublicUrl { get; }
    bool SmtpAyarli { get; }
    /// <summary>Düşük puan gelince İK'ya e-posta.</summary>
    bool IkMailAktif { get; }
    /// <summary>Teams/Slack gelen webhook adresi (boşsa kapalı).</summary>
    string? WebhookUrl { get; }
    bool HatirlatmaAktif { get; }
    int HatirlatmaGun { get; }
    int HatirlatmaMaks { get; }
    /// <summary>Mobil uygulamanın backend'i ile paylaşılan gizli anahtar (WebView otomatik giriş). Boşsa kapalı.</summary>
    string? MobilAnahtar { get; }
    /// <summary>"host:port" (SMTP ayarlı değilse boş); test mailinde gösterilir.</summary>
    string SmtpSunucu { get; }
}

/// <summary>İstek dilini ("tr" / "en") verir; kullanıcıya dönen metinler bu dile göre seçilir. İstek dışı çalışmalarda "tr".</summary>
public interface IDilBilgisi
{
    string Dil { get; }
}

public interface IWebhookGonderici
{
    Task GonderAsync(string metin, CancellationToken ct = default);
}


public record PersonelKaydi(string PersonelId, string Ad, string? Email, string? Departman);

/// <summary>Mevcut personel tablosunu salt okunur okur.</summary>
public interface IPersonelKaynagi
{
    Task<IReadOnlyList<PersonelKaydi>> OkuAsync(CancellationToken ct = default);
}
