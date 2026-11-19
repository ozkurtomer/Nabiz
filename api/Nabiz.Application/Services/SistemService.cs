using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface ISistemService
{
    Task<SistemDurumuDto> DurumAsync(CancellationToken ct);
}

/// <summary>Yönetici için "her şey yolunda mı?" özeti: veritabanı, son 24 saat mail sonuçları, açık özellikler.</summary>
public class SistemService(IUnitOfWork uow, IUygulamaAyarlari ayar, IZaman zaman) : ISistemService
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
}
