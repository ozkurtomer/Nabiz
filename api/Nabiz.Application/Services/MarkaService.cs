using System.Text.RegularExpressions;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;

namespace Nabiz.Application.Services;

public interface IMarkaService
{
    /// <summary>Şirket adı ve vurgu rengi (giriş ve cevaplama sayfaları giriş yapmadan okur).</summary>
    Task<MarkaDto> GetirAsync(CancellationToken ct);
    Task KaydetAsync(MarkaDto marka, CancellationToken ct);
}

public partial class MarkaService(IUnitOfWork uow, IAyarDeposu depo, IDenetimService denetim) : IMarkaService
{
    public const string VarsayilanAd = "Nabız";
    public const string VarsayilanRenk = "#1f4fc6";

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex RenkDeseni();

    public async Task<MarkaDto> GetirAsync(CancellationToken ct)
    {
        var ad = await depo.OkuAsync(AyarAnahtarlari.SirketAdi, ct);
        var renk = await depo.OkuAsync(AyarAnahtarlari.MarkaRenk, ct);
        var m = new MarkaDto(string.IsNullOrWhiteSpace(ad) ? VarsayilanAd : ad, string.IsNullOrWhiteSpace(renk) ? VarsayilanRenk : renk);
        EmailSablonu.Marka = (m.Ad, m.Renk);   // e-posta şablonları güncel markayı kullanır
        return m;
    }

    public async Task KaydetAsync(MarkaDto marka, CancellationToken ct)
    {
        var ad = (marka.Ad ?? "").Trim();
        if (ad.Length is 0 or > 60) throw new DogrulamaHatasi("Şirket adı 1-60 karakter olmalı.");
        if (!RenkDeseni().IsMatch(marka.Renk ?? "")) throw new DogrulamaHatasi("Renk #RRGGBB biçiminde olmalı.");
        await depo.YazAsync(AyarAnahtarlari.SirketAdi, ad, ct);
        await depo.YazAsync(AyarAnahtarlari.MarkaRenk, marka.Renk!.ToLowerInvariant(), ct);
        await uow.SaveChangesAsync(ct);
        EmailSablonu.Marka = (ad, marka.Renk!.ToLowerInvariant());
        await denetim.KaydetAsync("Marka ayarları değişti", $"{ad} {marka.Renk}", ct);
    }
}
