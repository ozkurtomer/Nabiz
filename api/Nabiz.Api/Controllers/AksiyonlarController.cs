using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;
using Nabiz.Domain.Enums;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/aksiyonlar"), Authorize(Roles = Roller.IK)]
public class AksiyonlarController(IAksiyonService aksiyonlar) : ControllerBase
{
    [HttpGet]
    public Task<List<AksiyonDto>> Liste([FromQuery] AksiyonDurumu? durum, CancellationToken ct) => aksiyonlar.ListeAsync(durum, ct);

    [HttpGet("ozet")]
    public Task<AksiyonOzetDto> Ozet(CancellationToken ct) => aksiyonlar.OzetAsync(ct);

    [HttpPost]
    public Task<AksiyonDto> Ekle(AksiyonKaydetIstek istek, CancellationToken ct) => aksiyonlar.EkleAsync(istek, ct);

    [HttpPut("{id:int}")]
    public Task<AksiyonDto> Guncelle(int id, AksiyonKaydetIstek istek, CancellationToken ct) => aksiyonlar.GuncelleAsync(id, istek, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken ct) { await aksiyonlar.SilAsync(id, ct); return NoContent(); }
}
