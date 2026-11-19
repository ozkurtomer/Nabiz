using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public interface IAyarService
{
    Task<AyarlarDto> GetirAsync(CancellationToken ct);
    Task KaydetAsync(AyarlarDto ayarlar, CancellationToken ct);
}

public class AyarService(IUnitOfWork uow) : IAyarService
{
    public const string EmojiEsik = nameof(EmojiEsik);
    public const string PuanEsik = nameof(PuanEsik);

    public async Task<AyarlarDto> GetirAsync(CancellationToken ct)
    {
        var d = await uow.Ayarlar.Query.ToDictionaryAsync(x => x.Anahtar, x => x.Deger, ct);
        int Al(string k, int varsayilan) => d.TryGetValue(k, out var v) && int.TryParse(v, out var n) ? n : varsayilan;
        return new AyarlarDto(Al(EmojiEsik, 2), Al(PuanEsik, 4));
    }

    public async Task KaydetAsync(AyarlarDto a, CancellationToken ct)
    {
        if (a.EmojiEsik is < 1 or > 5) throw new DogrulamaHatasi("Emoji eşiği 1-5 arasında olmalı.");
        if (a.PuanEsik is < 1 or > 10) throw new DogrulamaHatasi("Puan eşiği 1-10 arasında olmalı.");
        await Yaz(EmojiEsik, a.EmojiEsik, ct);
        await Yaz(PuanEsik, a.PuanEsik, ct);
        await uow.SaveChangesAsync(ct);
    }

    private async Task Yaz(string anahtar, int deger, CancellationToken ct)
    {
        var a = await uow.Ayarlar.FindAsync(anahtar, ct);
        if (a is null) uow.Ayarlar.Add(new AyarDeger { Anahtar = anahtar, Deger = deger.ToString() });
        else a.Deger = deger.ToString();
    }
}
