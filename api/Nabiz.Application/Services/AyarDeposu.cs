using Nabiz.Application.Abstractions;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public static class AyarAnahtarlari
{
    public const string SirketAdi = "SirketAdi";
    public const string MarkaRenk = "MarkaRenk";
    public const string SaklamaAy = "SaklamaAy";
    public const string AydinlatmaTr = "AydinlatmaTr";
    public const string AydinlatmaEn = "AydinlatmaEn";
    public const string SonSaklamaTemizligi = "SonSaklamaTemizligi";
    public const string SonHaftalikOzet = "SonHaftalikOzet";
}

public class AyarDeposu(IUnitOfWork uow) : IAyarDeposu
{
    public async Task<string?> OkuAsync(string anahtar, CancellationToken ct) => (await uow.Ayarlar.FindAsync(anahtar, ct))?.Deger;

    public async Task YazAsync(string anahtar, string deger, CancellationToken ct)
    {
        var a = await uow.Ayarlar.FindAsync(anahtar, ct);
        if (a is null) uow.Ayarlar.Add(new AyarDeger { Anahtar = anahtar, Deger = deger });
        else a.Deger = deger;
    }
}
