using Microsoft.Extensions.Configuration;
using Nabiz.Application.Abstractions;

namespace Nabiz.Infrastructure.Zaman;

/// <summary>Şirket saat dilimine (varsayılan Europe/Istanbul) göre zaman. Sunucu UTC olsa bile plan saatleri yerel çalışır.</summary>
public class SistemZamani : IZaman
{
    private readonly TimeZoneInfo _tz;

    public SistemZamani(IConfiguration cfg)
    {
        var ad = cfg["Zaman:SaatDilimi"] ?? "Europe/Istanbul";
        try { _tz = TimeZoneInfo.FindSystemTimeZoneById(ad); }
        catch (Exception) { _tz = TimeZoneInfo.CreateCustomTimeZone("TR", TimeSpan.FromHours(3), "TR", "TR"); }
    }

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Simdi => Yerel(DateTime.UtcNow);
    public DateTime Yerel(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _tz);
    public DateTime UtcCevir(DateTime yerel) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(yerel, DateTimeKind.Unspecified), _tz);
}

/// <summary>Uygulama davranış ayarları (appsettings / ortam değişkenleri).</summary>
public class UygulamaAyarlari(IConfiguration cfg) : IUygulamaAyarlari
{
    public string PublicUrl => cfg["Uygulama:PublicUrl"] ?? "http://localhost:4200";
    public bool SmtpAyarli => !string.IsNullOrWhiteSpace(cfg["Smtp:Host"]);
    public bool IkMailAktif => cfg.GetValue("Bildirim:IkMailAktif", true);
    public string? WebhookUrl => cfg["Bildirim:WebhookUrl"];
    public bool HatirlatmaAktif => cfg.GetValue("Hatirlatma:Aktif", true);
    public int HatirlatmaGun => cfg.GetValue("Hatirlatma:GunSonra", 2);
    public int HatirlatmaMaks => cfg.GetValue("Hatirlatma:Maks", 2);
    public string? MobilAnahtar => cfg["Mobil:Anahtar"];
    public string SmtpSunucu => string.IsNullOrWhiteSpace(cfg["Smtp:Host"]) ? "" : $"{cfg["Smtp:Host"]}:{cfg.GetValue("Smtp:Port", 25)}";
}
