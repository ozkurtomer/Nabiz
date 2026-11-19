using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nabiz.Application.Abstractions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IUyariBildirimService
{
    /// <summary>Düşük puan gelince İK'ya e-posta ve (varsa) Teams/Slack webhook bildirimi. Hata verirse çağıranı etkilemez.</summary>
    Task DusukPuanAsync(Gonderim gonderim, IReadOnlyList<(Soru Soru, Cevap Cevap)> dusukler, CancellationToken ct);
}

public class UyariBildirimService(IUnitOfWork uow, IEmailSender mail, IWebhookGonderici webhook, IUygulamaAyarlari ayar, ILogger<UyariBildirimService> log) : IUyariBildirimService
{
    public async Task DusukPuanAsync(Gonderim g, IReadOnlyList<(Soru Soru, Cevap Cevap)> dusukler, CancellationToken ct)
    {
        if (dusukler.Count == 0) return;
        var link = $"{ayar.PublicUrl.TrimEnd('/')}/uyarilar";

        if (ayar.IkMailAktif)
        {
            try
            {
                var ik = await uow.Kullanicilar.Query.Where(k => k.Aktif && k.Rol == Rol.IK).ToListAsync(ct);
                if (ik.Count == 0) ik = await uow.Kullanicilar.Query.Where(k => k.Aktif && k.Rol == Rol.Admin).ToListAsync(ct);
                var kisi = await uow.Kullanicilar.FindAsync(g.KullaniciId, ct);
                foreach (var alici in ik)
                {
                    try
                    {
                        var (konu, html) = EmailSablonu.IkUyari(alici, kisi!, dusukler, link);
                        await mail.GonderAsync(alici.Email, konu, html, ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException) { log.LogWarning("İK uyarı maili {Email} adresine gitmedi: {Mesaj}", alici.Email, ex.GetBaseException().Message); }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException) { log.LogWarning(ex, "İK uyarı bildirimi hazırlanamadı"); }
        }

        if (!string.IsNullOrWhiteSpace(ayar.WebhookUrl))
        {
            try { await webhook.GonderAsync($"⚠️ Nabız: yeni düşük puan uyarısı ({dusukler.Count} soru). Detaylar için: {link}", ct); }
            catch (Exception ex) when (ex is not OperationCanceledException) { log.LogWarning("Webhook bildirimi gitmedi: {Mesaj}", ex.GetBaseException().Message); }
        }
    }
}
