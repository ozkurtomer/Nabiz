using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/paketler"), Authorize(Roles = Roller.IK)]
public class PaketlerController(IPaketService paketler) : ControllerBase
{
    [HttpGet]
    public Task<List<PaketDto>> Liste(CancellationToken ct) => paketler.ListeAsync(ct);

    [HttpPost]
    public Task<PaketDto> Ekle(PaketKaydetIstek istek, CancellationToken ct) => paketler.EkleAsync(istek, ct);

    [HttpPut("{id:int}")]
    public Task<PaketDto> Guncelle(int id, PaketKaydetIstek istek, CancellationToken ct) => paketler.GuncelleAsync(id, istek, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken ct)
    {
        await paketler.SilAsync(id, ct);
        return NoContent();
    }
}
