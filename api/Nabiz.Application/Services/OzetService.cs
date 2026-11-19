using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nabiz.Application.Abstractions;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IOzetService
{
    /// <summary>Haftanın seçilen gün/saatinde İK'ya haftalık özet e-postası gönderir (haftada bir kez). Gönderilen alıcı sayısını döner.</summary>
    Task<int> GonderVarsaAsync(int gun, int saat, CancellationToken ct);
}

public class OzetService(IUnitOfWork uow, IAyarDeposu depo, IEmailSender mail, IZaman zaman, IUygulamaAyarlari ayar, IMarkaService marka, ILogger<OzetService> log) : IOzetService
{
    public async Task<int> GonderVarsaAsync(int gun, int saat, CancellationToken ct)
    {
        var simdi = zaman.Simdi;
        if ((int)simdi.DayOfWeek != gun || simdi.Hour < saat || !ayar.SmtpAyarli) return 0;
        var anahtar = $"{simdi:yyyy}-{System.Globalization.ISOWeek.GetWeekOfYear(simdi)}";
        if (await depo.OkuAsync(AyarAnahtarlari.SonHaftalikOzet, ct) == anahtar) return 0;
        // Önce işaretle: gönderim sırasında hata olsa da aynı hafta tekrar tekrar denenmesin
        await depo.YazAsync(AyarAnahtarlari.SonHaftalikOzet, anahtar, ct);
        await uow.SaveChangesAsync(ct);
        await marka.GetirAsync(ct);

        var veri = await HesaplaAsync(simdi, ct);
        var link = $"{ayar.PublicUrl.TrimEnd('/')}/raporlar";
        var n = 0;
        foreach (var ik in await uow.Kullanicilar.Query.Where(k => k.Aktif && k.Rol == Rol.IK).ToListAsync(ct))
        {
            try { var (konu, html) = EmailSablonu.HaftalikOzet(ik, veri, link); await mail.GonderAsync(ik.Email, konu, html, ct); n++; }
            catch (Exception ex) when (ex is not OperationCanceledException) { log.LogWarning("Haftalık özet {Email} adresine gitmedi: {Mesaj}", ik.Email, ex.GetBaseException().Message); }
        }
        return n;
    }

    private async Task<HaftalikOzetVerisi> HesaplaAsync(DateTime simdi, CancellationToken ct)
    {
        var bit = zaman.UtcNow; var bas = bit.AddDays(-7); var onceki = bas.AddDays(-7);
        var gonderimler = await uow.Gonderimler.Query.Include(g => g.Cevaplar).Include(g => g.Parti)
            .Where(g => g.GonderimZamani >= onceki && g.GonderimZamani < bit && !g.Parti!.Test && !g.Parti.Demo).ToListAsync(ct);
        var sorular = await uow.Sorular.Query.ToDictionaryAsync(s => s.Id, ct);
        double? Skor(IEnumerable<Domain.Entities.Gonderim> l)
        {
            var v = l.Where(g => g.Tamamlandi).SelectMany(g => g.Cevaplar).Select(c => sorular[c.SoruId].Skor(c.Deger)).ToList();
            return v.Count == 0 ? null : Math.Round(v.Average(), 1);
        }
        var bu = gonderimler.Where(g => g.GonderimZamani >= bas).ToList();
        var eski = gonderimler.Where(g => g.GonderimZamani < bas).ToList();
        var oran = bu.Count == 0 ? 0 : bu.Count(g => g.Tamamlandi) * 100.0 / bu.Count;
        var yeniUyari = await uow.Uyarilar.Query.CountAsync(u => u.Olusturma >= bas, ct);
        var geciken = await uow.Aksiyonlar.Query.CountAsync(a => a.SonTarih != null && a.SonTarih < simdi.Date && (a.Durum == AksiyonDurumu.Acik || a.Durum == AksiyonDurumu.Devam), ct);
        return new HaftalikOzetVerisi(Skor(bu), Skor(eski), oran, yeniUyari, geciken);
    }
}
