using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/auth"), AllowAnonymous, Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("kimlik")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>E-posta + şifre (şifresi olan hesaplar: ilk İK hesabı ve demo).</summary>
    [HttpPost("login")]
    public Task<LoginYanit> Login(LoginIstek istek, CancellationToken ct) => auth.LoginAsync(istek, ct);

    /// <summary>Şifresiz giriş: e-postaya 15 dakikalık giriş linki yollar. Hesap olsun olmasın aynı yanıtı döner.</summary>
    [HttpPost("giris-linki")]
    public async Task<IActionResult> GirisLinki(GirisLinkiIstek istek, CancellationToken ct)
    {
        await auth.GirisLinkiGonderAsync(istek.Email, ct);
        return Accepted();
    }

    [HttpPost("giris-dogrula")]
    public Task<LoginYanit> GirisDogrula(GirisDogrulaIstek istek, CancellationToken ct) => auth.GirisLinkiDogrulaAsync(istek.Token, ct);

    /// <summary>
    /// Mobil uygulamanın backend'i için (sunucudan sunucuya): <c>X-Mobil-Anahtar</c> başlığındaki paylaşılan anahtarla,
    /// kullanıcı adına tek kullanımlık WebView giriş adresi üretir. Anahtar tanımlı değilse uç kapalıdır.
    /// </summary>
    [HttpPost("mobil-giris")]
    public Task<MobilGirisYanit> MobilGiris(MobilGirisIstek istek, [FromHeader(Name = "X-Mobil-Anahtar")] string? anahtar, CancellationToken ct) =>
        auth.MobilGirisAsync(istek.Email, anahtar, ct);
}
