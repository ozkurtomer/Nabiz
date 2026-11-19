using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/sorular"), Authorize(Roles = Roller.AdminIK)]
public class SorularController(ISoruService sorular) : ControllerBase
{
    [HttpGet]
    public Task<List<SoruDto>> Liste(CancellationToken ct) => sorular.ListeAsync(ct);

    [HttpPost, Authorize(Roles = Roller.Admin)]
    public Task<SoruDto> Ekle(SoruKaydetIstek istek, CancellationToken ct) => sorular.EkleAsync(istek, ct);

    [HttpPut("{id:int}"), Authorize(Roles = Roller.Admin)]
    public Task<SoruDto> Guncelle(int id, SoruKaydetIstek istek, CancellationToken ct) => sorular.GuncelleAsync(id, istek, ct);
}
