using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/planlar"), Authorize(Roles = Roller.AdminIK)]
public class PlanlarController(IPlanService planlar, IGonderimService gonderim) : ControllerBase
{
    [HttpGet]
    public Task<List<PlanDto>> Liste(CancellationToken ct) => planlar.ListeAsync(ct);

    [HttpPost]
    public Task<PlanDto> Ekle(PlanKaydetIstek istek, CancellationToken ct) => planlar.EkleAsync(istek, ct);

    [HttpPut("{id:int}")]
    public Task<PlanDto> Guncelle(int id, PlanKaydetIstek istek, CancellationToken ct) => planlar.GuncelleAsync(id, istek, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken ct)
    {
        await planlar.SilAsync(id, ct);
        return NoContent();
    }

    /// <summary>Planı zamanını beklemeden hemen çalıştırır.</summary>
    [HttpPost("{id:int}/simdi-gonder")]
    public Task<GonderimSonucu> SimdiGonder(int id, CancellationToken ct) => gonderim.PlaniSimdiGonderAsync(id, ct);
}
