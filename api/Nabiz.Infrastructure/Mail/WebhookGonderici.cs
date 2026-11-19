using System.Net.Http.Json;
using Nabiz.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Nabiz.Infrastructure.Mail;

/// <summary>Teams (Incoming Webhook / Workflows) ve Slack ile uyumlu basit metin bildirimi: {"text": "..."}.</summary>
public class WebhookGonderici(IHttpClientFactory http, IConfiguration cfg) : IWebhookGonderici
{
    public async Task GonderAsync(string metin, CancellationToken ct = default)
    {
        var url = cfg["Bildirim:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(url)) return;
        using var istemci = http.CreateClient();
        istemci.Timeout = TimeSpan.FromSeconds(10);
        using var yanit = await istemci.PostAsJsonAsync(url, new { text = metin }, ct);
        yanit.EnsureSuccessStatusCode();
    }
}
