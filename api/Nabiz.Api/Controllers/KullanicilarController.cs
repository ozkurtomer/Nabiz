using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

/// <summary>Kullanıcılar personel tablosundan gelir; burada yalnızca rol/aktiflik yönetilir.</summary>
[ApiController, Route("api/kullanicilar")]
public class KullanicilarController(IKullaniciService kullanicilar) : ControllerBase
{
    /// <summary>Kişi seçiciler (gönderim) için kullanıcı listesi.</summary>
    [HttpGet, Authorize(Roles = Roller.IK)]
    public Task<List<KullaniciDto>> Liste([FromQuery] string? ara, CancellationToken ct) => kullanicilar.ListeAsync(ara, ct);

    [HttpPut("{id:int}"), Authorize(Roles = Roller.IK)]
    public async Task<IActionResult> Guncelle(int id, KullaniciGuncelleIstek istek, CancellationToken ct)
    {
        await kullanicilar.GuncelleAsync(id, istek, ct);
        return NoContent();
    }

    /// <summary>Personel tablosundan kullanıcıları hemen senkronize eder (arka planda zaten periyodik çalışır).</summary>
    [HttpPost("personel-aktar"), Authorize(Roles = Roller.IK)]
    public Task<PersonelAktarimSonucu> PersonelAktar(CancellationToken ct) => kullanicilar.PersonelAktarAsync(ct);
}
