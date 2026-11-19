using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/kvkk"), Authorize(Roles = Roller.IK)]
public class KvkkController(IKvkkService kvkk) : ControllerBase
{
    [HttpGet]
    public Task<KvkkAyarDto> Getir(CancellationToken ct) => kvkk.AyarGetirAsync(ct);

    [HttpPut]
    public async Task<IActionResult> Kaydet(KvkkAyarDto ayar, CancellationToken ct) { await kvkk.AyarKaydetAsync(ayar, ct); return NoContent(); }

    [HttpGet("saklama")]
    public Task<SaklamaOnizleme> Onizle(CancellationToken ct) => kvkk.SaklamaOnizleAsync(ct);

    [HttpPost("saklama")]
    public Task<SaklamaSonucu> Uygula(CancellationToken ct) => kvkk.SaklamaUygulaAsync(ct);

    [HttpGet("kisi/{id:int}/disa-aktar")]
    public async Task<IActionResult> DisaAktar(int id, CancellationToken ct) =>
        File(await kvkk.KisiDisaAktarAsync(id, ct), "text/csv; charset=utf-8", $"nabiz-kisi-{id}-{DateTime.Now:yyyyMMdd}.csv");

    [HttpDelete("kisi/{id:int}")]
    public Task<KisiSilSonucu> KisiSil(int id, CancellationToken ct) => kvkk.KisiSilAsync(id, ct);
}
