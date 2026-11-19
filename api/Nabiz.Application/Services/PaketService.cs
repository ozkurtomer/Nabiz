using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public interface IPaketService
{
    Task<List<PaketDto>> ListeAsync(CancellationToken ct);
    Task<PaketDto> EkleAsync(PaketKaydetIstek istek, CancellationToken ct);
    Task<PaketDto> GuncelleAsync(int id, PaketKaydetIstek istek, CancellationToken ct);
    Task SilAsync(int id, CancellationToken ct);
}

public class PaketService(IUnitOfWork uow) : IPaketService
{
    private IQueryable<SoruPaketi> Sorgu => uow.Paketler.Query.Include(p => p.Sorular).ThenInclude(s => s.Soru);

    public static PaketDto Dto(SoruPaketi p) =>
        new(p.Id, p.Ad, p.AdEn, p.Aciklama, p.AciklamaEn, p.Aktif, p.Sorular.OrderBy(s => s.Sira).Select(s => SoruService.Dto(s.Soru!)).ToList());

    public async Task<List<PaketDto>> ListeAsync(CancellationToken ct) =>
        (await Sorgu.OrderBy(p => p.Id).ToListAsync(ct)).Select(Dto).ToList();

    public async Task<PaketDto> EkleAsync(PaketKaydetIstek i, CancellationToken ct)
    {
        await DogrulaAsync(i, ct);
        var p = new SoruPaketi();
        Uygula(p, i);
        uow.Paketler.Add(p);
        await uow.SaveChangesAsync(ct);
        return await Getir(p.Id, ct);
    }

    public async Task<PaketDto> GuncelleAsync(int id, PaketKaydetIstek i, CancellationToken ct)
    {
        await DogrulaAsync(i, ct);
        var p = await Sorgu.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new BulunamadiHatasi();
        p.Sorular.Clear();
        Uygula(p, i);
        await uow.SaveChangesAsync(ct);
        return await Getir(id, ct);
    }

    public async Task SilAsync(int id, CancellationToken ct)
    {
        var p = await uow.Paketler.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        if (await uow.GonderimPlanlari.Query.AnyAsync(x => x.PaketId == id, ct) || await uow.GonderimPartileri.Query.AnyAsync(x => x.PaketId == id, ct))
            throw new CakismaHatasi("Bu paket planlarda veya geçmiş gönderimlerde kullanılıyor; silmek yerine pasif yapabilirsin.");
        uow.Paketler.Remove(p);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<PaketDto> Getir(int id, CancellationToken ct) => Dto(await Sorgu.FirstAsync(p => p.Id == id, ct));

    private static void Uygula(SoruPaketi p, PaketKaydetIstek i)
    {
        p.Ad = (i.Ad ?? "").Trim(); p.AdEn = (i.AdEn ?? "").Trim();
        p.Aciklama = string.IsNullOrWhiteSpace(i.Aciklama) ? null : i.Aciklama.Trim();
        p.AciklamaEn = string.IsNullOrWhiteSpace(i.AciklamaEn) ? null : i.AciklamaEn.Trim();
        p.Aktif = i.Aktif;
        var sira = 0;
        foreach (var soruId in i.SoruIdleri.Distinct()) p.Sorular.Add(new PaketSoru { SoruId = soruId, Sira = sira++ });
    }

    private async Task DogrulaAsync(PaketKaydetIstek i, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(i.Ad) && string.IsNullOrWhiteSpace(i.AdEn)) throw new DogrulamaHatasi("Paket adı zorunlu (Türkçe ya da İngilizce).");
        var ids = i.SoruIdleri.Distinct().ToList();
        if (ids.Count == 0) throw new DogrulamaHatasi("Pakete en az bir soru eklemelisin.");
        if (await uow.Sorular.Query.CountAsync(s => ids.Contains(s.Id), ct) != ids.Count) throw new DogrulamaHatasi("Seçilen sorulardan bazıları bulunamadı.");
    }
}
