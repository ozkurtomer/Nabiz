using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/cevap")]
public class CevapController(ICevapService cevaplar, ICurrentUser kullanici) : ControllerBase
{
    /// <summary>E-postadaki linkten gelen çalışan: kişiye özel gizli token yeterli, giriş gerekmez.</summary>
    [HttpGet("{token}"), AllowAnonymous]
    public Task<CevapSayfasiDto> Getir(string token, CancellationToken ct) => cevaplar.GetirAsync(token, ct);

    [HttpPost("{token}"), AllowAnonymous]
    public Task<CevapKaydetYanit> Kaydet(string token, CevapKaydetIstek istek, CancellationToken ct) => cevaplar.KaydetAsync(token, istek, ct);

    /// <summary>Oturum açan kullanıcıya gelen paketler.</summary>
    [HttpGet("benim"), Authorize]
    public Task<List<BenimSoruDto>> Benim(CancellationToken ct) => cevaplar.BenimAsync(kullanici.Id, ct);

    /// <summary>Deneme: paketi e-posta atmadan kendine gönderir ve cevaplama linkini üretir.</summary>
    [HttpPost("test/{paketId:int}"), Authorize(Roles = Roller.Admin)]
    public Task<TestGonderimYanit> TestGonderimi(int paketId, CancellationToken ct) => cevaplar.TestGonderimiOlusturAsync(kullanici.Id, paketId, ct);
}
