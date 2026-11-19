using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

/// <summary>Oturum açan kullanıcının kendi tercihleri.</summary>
[ApiController, Route("api/profil"), Authorize]
public class ProfilController(IKullaniciService kullanicilar, ICurrentUser ben) : ControllerBase
{
    /// <summary>Arayüz ve e-posta dilini kaydeder ("tr" / "en"); sonraki mailler bu dilde gelir.</summary>
    [HttpPut("dil")]
    public async Task<IActionResult> Dil(ProfilDilIstek istek, CancellationToken ct)
    {
        await kullanicilar.DilDegistirAsync(ben.Id, istek.Dil, ct);
        return NoContent();
    }
}
