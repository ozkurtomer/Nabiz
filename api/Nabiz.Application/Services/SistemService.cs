using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface ISistemService
{
    Task<SistemDurumuDto> DurumAsync(CancellationToken ct);
    /// <summary>SMTP ayarlarını doğrulamak için (boşsa kendi adresine) test e-postası gönderir; hata olursa nedenini döner.</summary>
    Task<TestMailYanit> TestMailiGonderAsync(string? adres, CancellationToken ct);
}

/// <summary>İK için "her şey yolunda mı?" özeti: veritabanı, son 24 saat mail sonuçları, açık özellikler.</summary>
public class SistemService(IUnitOfWork uow, IUygulamaAyarlari ayar, IZaman zaman, IEmailSender mail, ICurrentUser me, IDenetimService denetim, IDilBilgisi? dilBilgisi = null) : ISistemService
{
    public const string SonSenkronAnahtari = "SonPersonelSenkron";

    public async Task<SistemDurumuDto> DurumAsync(CancellationToken ct)
    {
        var db = true; int gonderilen = 0, hatali = 0; DateTime? senkron = null;
        try
        {
            var sinir = zaman.UtcNow.AddHours(-24);
            var son = await uow.Gonderimler.Query.Where(g => g.GonderimZamani >= sinir && !g.Parti!.Demo && !g.Parti.Test)
                .GroupBy(g => g.Durum).Select(x => new { Durum = x.Key, N = x.Count() }).ToListAsync(ct);
            gonderilen = son.Where(x => x.Durum == GonderimDurumu.Gonderildi).Sum(x => x.N);
            hatali = son.Where(x => x.Durum == GonderimDurumu.Hata).Sum(x => x.N);
            var a = await uow.Ayarlar.FindAsync(SonSenkronAnahtari, ct);
            if (a is not null && DateTime.TryParse(a.Deger, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d)) senkron = zaman.Yerel(d);
        }
        catch { db = false; }

        return new SistemDurumuDto(db, gonderilen, hatali, ayar.SmtpAyarli, ayar.IkMailAktif, !string.IsNullOrWhiteSpace(ayar.WebhookUrl),
            ayar.HatirlatmaAktif, !string.IsNullOrWhiteSpace(ayar.MobilAnahtar), senkron);
    }

    public async Task<TestMailYanit> TestMailiGonderAsync(string? adres, CancellationToken ct)
    {
        var dil = dilBilgisi?.Dil ?? "tr";
        var ben = await uow.Kullanicilar.FindAsync(me.Id, ct) ?? throw new YetkisizHatasi();
        var hedef = string.IsNullOrWhiteSpace(adres) ? ben.Email : adres.Trim();
        try { _ = new System.Net.Mail.MailAddress(hedef); }
        catch (FormatException) { throw new DogrulamaHatasi("Geçerli bir e-posta adresi gir."); }

        var sunucu = string.IsNullOrWhiteSpace(ayar.SmtpSunucu) ? "—" : ayar.SmtpSunucu;
        TestMailYanit yanit;
        try
        {
            var (konu, html) = EmailSablonu.Test(dil, sunucu);
            await mail.GonderAsync(hedef, konu, html, ct);
            yanit = new TestMailYanit(true, hedef, sunucu);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            yanit = new TestMailYanit(false, ex.GetBaseException().Message, sunucu);
        }
        await denetim.KaydetAsync("Test maili gönderildi", $"{hedef} · {(yanit.Basarili ? "başarılı" : "hata")} · {sunucu}", ct);
        return yanit;
    }
}
