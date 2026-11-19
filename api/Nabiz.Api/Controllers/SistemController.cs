using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

/// <summary>Sistem durumu ve denetim kaydı (yalnızca Admin).</summary>
[ApiController, Route("api/sistem"), Authorize(Roles = Roller.Admin)]
public class SistemController(ISistemService sistem, IDenetimService denetim) : ControllerBase
{
    [HttpGet("durum")]
    public Task<SistemDurumuDto> Durum(CancellationToken ct) => sistem.DurumAsync(ct);

    [HttpGet("denetim")]
    public Task<List<DenetimDto>> Denetim(CancellationToken ct) => denetim.ListeAsync(ct);
}
