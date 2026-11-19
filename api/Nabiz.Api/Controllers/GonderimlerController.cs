using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

/// <summary>Anlık (plansız) gönderim ve gönderim bazlı cevap raporları. Kişi bazlı veri içerdiği için yalnızca Admin/İK.</summary>
[ApiController, Route("api/gonderimler"), Authorize(Roles = Roller.AdminIK)]
public class GonderimlerController(IGonderimService gonderim) : ControllerBase
{
    [HttpPost("anlik")]
    public Task<AnlikGonderYanit> Anlik(AnlikGonderIstek istek, CancellationToken ct) => gonderim.AnlikGonderAsync(istek, ct);

    [HttpGet]
    public Task<List<PartiDto>> Liste(CancellationToken ct) => gonderim.PartilerAsync(ct);

    [HttpGet("{id:int}")]
    public Task<PartiDetayDto> Detay(int id, CancellationToken ct) => gonderim.PartiDetayAsync(id, ct);

    [HttpGet("{id:int}/csv")]
    public async Task<FileContentResult> Csv(int id, CancellationToken ct) =>
        File(await gonderim.PartiCsvAsync(id, ct), "text/csv; charset=utf-8", $"gonderim-{id}.csv");

    [HttpPost("{id:int}/tekrar-dene")]
    public Task<GonderimSonucu> TekrarDene(int id, CancellationToken ct) => gonderim.HatalilariTekrarDeneAsync(id, ct);
}
