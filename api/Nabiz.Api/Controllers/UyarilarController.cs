using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/uyarilar"), Authorize(Roles = Roller.IK)]
public class UyarilarController(IUyariService uyarilar) : ControllerBase
{
    [HttpGet]
    public Task<List<UyariDto>> Liste(CancellationToken ct) => uyarilar.ListeAsync(ct);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, UyariGuncelleIstek istek, CancellationToken ct)
    {
        await uyarilar.GuncelleAsync(id, istek, ct);
        return NoContent();
    }
}
