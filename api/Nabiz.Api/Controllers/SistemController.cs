using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

/// <summary>Sistem durumu ve denetim kaydı (yalnızca İK).</summary>
[ApiController, Route("api/sistem"), Authorize(Roles = Roller.IK)]
public class SistemController(ISistemService sistem, IDenetimService denetim) : ControllerBase
{
    [HttpGet("durum")]
    public Task<SistemDurumuDto> Durum(CancellationToken ct) => sistem.DurumAsync(ct);

    /// <summary>SMTP ayarlarını doğrulamak için test e-postası yollar (adres boşsa kendi adresine).</summary>
    [HttpPost("test-mail")]
    public Task<TestMailYanit> TestMail(TestMailIstek istek, CancellationToken ct) => sistem.TestMailiGonderAsync(istek.Adres, ct);

    [HttpGet("denetim")]
    public Task<List<DenetimDto>> Denetim(CancellationToken ct) => denetim.ListeAsync(ct);
}
