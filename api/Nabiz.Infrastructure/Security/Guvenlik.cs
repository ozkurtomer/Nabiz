using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nabiz.Application.Abstractions;
using Nabiz.Domain.Entities;

namespace Nabiz.Infrastructure.Security;

public class JwtOptions
{
    public const string Bolum = "Jwt";
    public string Issuer { get; set; } = "Nabiz";
    public string Audience { get; set; } = "Nabiz";
    public string Key { get; set; } = "";
    public int SureDakika { get; set; } = 480;
}

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hashle(string sifre) => BCrypt.Net.BCrypt.HashPassword(sifre);
    public bool Dogrula(string sifre, string hash) => BCrypt.Net.BCrypt.Verify(sifre, hash);
}

public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _o = options.Value;
    private const string GirisAmaci = "giris-linki";
    private SymmetricSecurityKey Anahtar => new(Encoding.UTF8.GetBytes(_o.Key));

    public string Olustur(Kullanici k) => Yaz(new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddMinutes(_o.SureDakika),
        Subject = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, k.Id.ToString()),
            new Claim(ClaimTypes.Email, k.Email),
            new Claim(ClaimTypes.Role, k.Rol.ToString()),
        ]),
    });

    public string GirisLinkiTokeniOlustur(Kullanici k) => Yaz(new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddMinutes(15),
        Subject = new ClaimsIdentity([new Claim("uid", k.Id.ToString()), new Claim("amac", GirisAmaci)]),
    });

    public async Task<int?> GirisLinkiTokeniDogrulaAsync(string token)
    {
        var sonuc = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = _o.Issuer, ValidAudience = _o.Audience, IssuerSigningKey = Anahtar, ClockSkew = TimeSpan.FromMinutes(1),
        });
        if (!sonuc.IsValid) return null;
        var jwt = (JsonWebToken)sonuc.SecurityToken;
        if (jwt.TryGetClaim("amac", out var amac) is false || amac.Value != GirisAmaci) return null;
        return jwt.TryGetClaim("uid", out var uid) && int.TryParse(uid.Value, out var id) ? id : null;
    }

    private string Yaz(SecurityTokenDescriptor d)
    {
        d.Issuer = _o.Issuer; d.Audience = _o.Audience;
        d.SigningCredentials = new SigningCredentials(Anahtar, SecurityAlgorithms.HmacSha256);
        return new JsonWebTokenHandler().CreateToken(d);
    }
}
