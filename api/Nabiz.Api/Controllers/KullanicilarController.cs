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
    /// <summary>Kişi seçiciler (anlık gönderim, plan) için İK de listeyi görebilir.</summary>
    [HttpGet, Authorize(Roles = Roller.AdminIK)]
    public Task<List<KullaniciDto>> Liste([FromQuery] string? ara, CancellationToken ct) => kullanicilar.ListeAsync(ara, ct);

    [HttpPut("{id:int}"), Authorize(Roles = Roller.Admin)]
    public async Task<IActionResult> Guncelle(int id, KullaniciGuncelleIstek istek, CancellationToken ct)
    {
        await kullanicilar.GuncelleAsync(id, istek, ct);
        return NoContent();
    }

    /// <summary>Personel tablosundan kullanıcıları hemen senkronize eder (arka planda zaten periyodik çalışır).</summary>
    [HttpPost("personel-aktar"), Authorize(Roles = Roller.Admin)]
    public Task<PersonelAktarimSonucu> PersonelAktar(CancellationToken ct) => kullanicilar.PersonelAktarAsync(ct);
}
