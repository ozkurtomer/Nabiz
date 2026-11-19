using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/ayarlar"), Authorize(Roles = Roller.IK)]
public class AyarlarController(IAyarService ayarlar) : ControllerBase
{
    [HttpGet]
    public Task<AyarlarDto> Getir(CancellationToken ct) => ayarlar.GetirAsync(ct);

    [HttpPut]
    public async Task<IActionResult> Kaydet(AyarlarDto dto, CancellationToken ct)
    {
        await ayarlar.KaydetAsync(dto, ct);
        return NoContent();
    }
}
