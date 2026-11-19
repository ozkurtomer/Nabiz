using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public interface ISoruService
{
    Task<List<SoruDto>> ListeAsync(CancellationToken ct);
    Task<SoruDto> EkleAsync(SoruKaydetIstek istek, CancellationToken ct);
    Task<SoruDto> GuncelleAsync(int id, SoruKaydetIstek istek, CancellationToken ct);
}

public class SoruService(IUnitOfWork uow) : ISoruService
{
    public static SoruDto Dto(Soru s) => new(s.Id, s.MetinTr, s.MetinEn, s.Tip, s.Aktif, s.TakipMetniTr, s.TakipMetniEn);

    public async Task<List<SoruDto>> ListeAsync(CancellationToken ct) =>
        (await uow.Sorular.Query.OrderBy(x => x.Id).ToListAsync(ct)).Select(Dto).ToList();

    public async Task<SoruDto> EkleAsync(SoruKaydetIstek i, CancellationToken ct)
    {
        Dogrula(i);
        var s = new Soru { MetinTr = (i.MetinTr ?? "").Trim(), MetinEn = (i.MetinEn ?? "").Trim(), Tip = i.Tip, Aktif = i.Aktif, TakipMetniTr = Bos(i.TakipMetniTr), TakipMetniEn = Bos(i.TakipMetniEn) };
        uow.Sorular.Add(s);
        await uow.SaveChangesAsync(ct);
        return Dto(s);
    }

    public async Task<SoruDto> GuncelleAsync(int id, SoruKaydetIstek i, CancellationToken ct)
    {
        Dogrula(i);
        var s = await uow.Sorular.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        s.MetinTr = (i.MetinTr ?? "").Trim(); s.MetinEn = (i.MetinEn ?? "").Trim(); s.Tip = i.Tip; s.Aktif = i.Aktif;
        s.TakipMetniTr = Bos(i.TakipMetniTr); s.TakipMetniEn = Bos(i.TakipMetniEn);
        await uow.SaveChangesAsync(ct);
        return Dto(s);
    }

    private static string? Bos(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();

    private static void Dogrula(SoruKaydetIstek i)
    {
        if (string.IsNullOrWhiteSpace(i.MetinTr) && string.IsNullOrWhiteSpace(i.MetinEn))
            throw new DogrulamaHatasi("Soru metnini Türkçe ya da İngilizce olarak yazmalısın (en az biri zorunlu).");
    }
}
