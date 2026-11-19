using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/marka")]
public class MarkaController(IMarkaService marka) : ControllerBase
{
    /// <summary>Şirket adı ve rengi: giriş ve cevaplama sayfaları oturumsuz okur (gizli bilgi içermez).</summary>
    [HttpGet, AllowAnonymous]
    public Task<MarkaDto> Getir(CancellationToken ct) => marka.GetirAsync(ct);

    [HttpPut, Authorize(Roles = Roller.IK)]
    public async Task<IActionResult> Kaydet(MarkaDto istek, CancellationToken ct) { await marka.KaydetAsync(istek, ct); return NoContent(); }
}
