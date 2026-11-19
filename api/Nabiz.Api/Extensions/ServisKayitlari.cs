using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Nabiz.Infrastructure.Security;

namespace Nabiz.Api.Extensions;

public static class ServisKayitlari
{
    public static IServiceCollection AddJwtKimlikDogrulama(this IServiceCollection s, IConfiguration cfg)
    {
        var jwt = cfg.GetSection(JwtOptions.Bolum).Get<JwtOptions>() ?? new JwtOptions();
        if (jwt.Key.Length < 32) throw new InvalidOperationException("Jwt:Key en az 32 karakter olmalı.");

        s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = true;
                // E-postayla gönderilen tek amaçlı giriş-linki token'ları API oturumu olarak kullanılamaz.
                o.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.HasClaim(c => c.Type == "amac") == true) ctx.Fail("Geçersiz token türü.");
                        return Task.CompletedTask;
                    },
                };
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
        s.AddAuthorization();
        return s;
    }

    public static IServiceCollection AddCorsAyarlari(this IServiceCollection s, IConfiguration cfg)
    {
        var origins = cfg.GetSection("Cors:Origins").Get<string[]>() ?? [];
        s.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
        return s;
    }
}
