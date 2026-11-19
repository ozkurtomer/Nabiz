using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nabiz.Application.Services;

namespace Nabiz.Infrastructure.Zamanlayici;

/// <summary>
/// Her dakika: vakti gelen gönderim planlarını işler (e-posta gönderir) ve cevaplamayanlara hatırlatma yollar.
/// Her N dakikada bir: personel tablosundan kullanıcıları senkronize eder.
/// </summary>
public class ArkaPlanIsleri(IServiceScopeFactory scopes, IConfiguration cfg, ILogger<ArkaPlanIsleri> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var gonderimAktif = cfg.GetValue("Zamanlayici:GonderimAktif", true);
        var senkronAktif = cfg.GetValue("Zamanlayici:PersonelSenkronAktif", true);
        var kontrol = TimeSpan.FromSeconds(cfg.GetValue("Zamanlayici:KontrolSaniye", 60));
        var senkronAralik = TimeSpan.FromMinutes(cfg.GetValue("Zamanlayici:PersonelSenkronDakika", 60));
        var sonSenkron = DateTime.MinValue;

        while (!ct.IsCancellationRequested)
        {
            if (senkronAktif && DateTime.UtcNow - sonSenkron >= senkronAralik)
            {
                sonSenkron = DateTime.UtcNow;
                await Calistir("Personel senkronu", async sp =>
                {
                    var r = await sp.GetRequiredService<IKullaniciService>().PersonelAktarAsync(ct);
                    log.LogInformation("Personel senkronu: {Yeni} yeni, {Guncel} güncel, {Atlanan} atlandı.", r.Yeni, r.Guncel, r.Atlanan);
                });
            }
            if (gonderimAktif)
            {
                await Calistir("Gönderim planları", async sp =>
                {
                    var n = await sp.GetRequiredService<IGonderimService>().VadesiGelenleriIsleAsync(ct);
                    if (n > 0) log.LogInformation("Zamanlayıcı {N} gönderim üretti.", n);
                });
                await Calistir("Hatırlatmalar", async sp =>
                {
                    var n = await sp.GetRequiredService<IGonderimService>().HatirlatmalariGonderAsync(ct);
                    if (n > 0) log.LogInformation("{N} hatırlatma maili gönderildi.", n);
                });
            }
            if (cfg.GetValue("Ozet:Aktif", true))
                await Calistir("Haftalık özet", async sp =>
                {
                    var n = await sp.GetRequiredService<IOzetService>().GonderVarsaAsync(cfg.GetValue("Ozet:Gun", 1), cfg.GetValue("Ozet:Saat", 9), ct);
                    if (n > 0) log.LogInformation("Haftalık özet {N} İK kullanıcısına gönderildi.", n);
                });
            await Calistir("Saklama süresi temizliği", async sp =>
            {
                var r = await sp.GetRequiredService<IKvkkService>().SaklamaGunlukAsync(ct);
                if (r is { Gonderim: > 0 }) log.LogInformation("Saklama süresi dolan {N} gönderim silindi.", r.Gonderim);
            });
            try { await Task.Delay(kontrol, ct); } catch (OperationCanceledException) { break; }
        }
    }

    private async Task Calistir(string ad, Func<IServiceProvider, Task> is_)
    {
        try { using var scope = scopes.CreateScope(); await is_(scope.ServiceProvider); }
        catch (Exception ex) when (ex is not OperationCanceledException) { log.LogError(ex, "{Is} sırasında hata", ad); }
    }
}
