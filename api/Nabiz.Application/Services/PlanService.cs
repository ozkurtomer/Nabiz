using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IPlanService
{
    Task<List<PlanDto>> ListeAsync(CancellationToken ct);
    Task<PlanDto> EkleAsync(PlanKaydetIstek istek, CancellationToken ct);
    Task<PlanDto> GuncelleAsync(int id, PlanKaydetIstek istek, CancellationToken ct);
    Task SilAsync(int id, CancellationToken ct);
}

public class PlanService(IUnitOfWork uow, IDilBilgisi? dilBilgisi = null) : IPlanService
{
    private string Lang => dilBilgisi?.Dil ?? "tr";

    private PlanDto Dto(GonderimPlani p, IReadOnlyDictionary<int, string> isimler) =>
        new(p.Id, p.PaketId, p.Paket?.AdGoster(Lang), p.HedefTipi, p.HedefDeger, HedefYardimcisi.Aciklama(p.HedefTipi, p.HedefDeger, isimler, Lang),
            p.Baslangic, p.SiklikGun, p.Saat.ToString(@"hh\:mm"), p.Aktif, p.SonCalisma);

    private async Task<Dictionary<int, string>> IsimleriGetirAsync(IEnumerable<GonderimPlani> planlar, CancellationToken ct)
    {
        var ids = planlar.Where(p => p.HedefTipi == HedefTipi.Kisi).SelectMany(p => Domain.Hedef.KisiIdleri(p.HedefDeger)).Distinct().ToList();
        return ids.Count == 0 ? [] : await uow.Kullanicilar.Query.Where(k => ids.Contains(k.Id)).ToDictionaryAsync(k => k.Id, k => k.Ad, ct);
    }

    public async Task<List<PlanDto>> ListeAsync(CancellationToken ct)
    {
        var liste = await uow.GonderimPlanlari.Query.Include(x => x.Paket).OrderByDescending(x => x.Id).ToListAsync(ct);
        var isimler = await IsimleriGetirAsync(liste, ct);
        return liste.Select(p => Dto(p, isimler)).ToList();
    }

    public async Task<PlanDto> EkleAsync(PlanKaydetIstek i, CancellationToken ct)
    {
        var p = new GonderimPlani();
        await UygulaAsync(p, i, ct);
        uow.GonderimPlanlari.Add(p);
        await uow.SaveChangesAsync(ct);
        return await GetirAsync(p.Id, ct);
    }

    public async Task<PlanDto> GuncelleAsync(int id, PlanKaydetIstek i, CancellationToken ct)
    {
        var p = await uow.GonderimPlanlari.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        await UygulaAsync(p, i, ct);
        await uow.SaveChangesAsync(ct);
        return await GetirAsync(id, ct);
    }

    public async Task SilAsync(int id, CancellationToken ct)
    {
        var p = await uow.GonderimPlanlari.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        uow.GonderimPlanlari.Remove(p);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<PlanDto> GetirAsync(int id, CancellationToken ct)
    {
        var p = await uow.GonderimPlanlari.Query.Include(x => x.Paket).FirstAsync(x => x.Id == id, ct);
        return Dto(p, await IsimleriGetirAsync([p], ct));
    }

    private async Task UygulaAsync(GonderimPlani p, PlanKaydetIstek i, CancellationToken ct)
    {
        if (!TimeSpan.TryParse(i.Saat, out var saat)) throw new DogrulamaHatasi("Saat biçimi SS:dd olmalı.");
        if (i.SiklikGun < 0) throw new DogrulamaHatasi("Sıklık geçersiz (0 = bir kez, 1+ = kaç günde bir).");
        if (!await uow.Paketler.Query.AnyAsync(x => x.Id == i.PaketId, ct)) throw new DogrulamaHatasi("Paket bulunamadı.");
        await HedefYardimcisi.DogrulaAsync(uow, i.HedefTipi, i.HedefDeger, ct);
        p.PaketId = i.PaketId; p.HedefTipi = i.HedefTipi;
        p.HedefDeger = i.HedefTipi == HedefTipi.Hepsi ? null : i.HedefDeger;
        p.Baslangic = i.Baslangic.Date; p.SiklikGun = i.SiklikGun; p.Saat = saat; p.Aktif = i.Aktif;
    }
}
