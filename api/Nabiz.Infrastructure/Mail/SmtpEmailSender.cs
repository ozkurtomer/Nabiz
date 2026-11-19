using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using Nabiz.Application.Abstractions;

namespace Nabiz.Infrastructure.Mail;

public class SmtpOptions
{
    public const string Bolum = "Smtp";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 25;
    public string? Kullanici { get; set; }
    public string? Sifre { get; set; }
    public bool EnableSsl { get; set; }
    public string Gonderen { get; set; } = "nabiz@localhost";
    public string GonderenAd { get; set; } = "Nabız";
}

/// <summary>Şirketin mevcut SMTP sunucusu üzerinden e-posta gönderir (ayarlar appsettings "Smtp" bölümünden).</summary>
public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _o = options.Value;

    public async Task GonderAsync(string kime, string konu, string htmlGovde, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_o.Host)) throw new InvalidOperationException("SMTP ayarlanmamış (Smtp:Host boş).");

        using var client = new SmtpClient(_o.Host, _o.Port) { EnableSsl = _o.EnableSsl, Timeout = 20000 };
        if (!string.IsNullOrWhiteSpace(_o.Kullanici)) client.Credentials = new NetworkCredential(_o.Kullanici, _o.Sifre);

        using var mesaj = new MailMessage
        {
            From = new MailAddress(_o.Gonderen, _o.GonderenAd, Encoding.UTF8),
            Subject = konu, SubjectEncoding = Encoding.UTF8,
            Body = htmlGovde, BodyEncoding = Encoding.UTF8, IsBodyHtml = true,
        };
        mesaj.To.Add(kime);
        await client.SendMailAsync(mesaj, ct);
    }
}
