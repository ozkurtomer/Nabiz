using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nabiz.Application.Abstractions;
using Nabiz.Infrastructure.Mail;
using Nabiz.Infrastructure.Persistence;
using Nabiz.Infrastructure.Personel;
using Nabiz.Infrastructure.Security;
using Nabiz.Infrastructure.Zaman;
using Nabiz.Infrastructure.Zamanlayici;

namespace Nabiz.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration cfg)
    {
        s.AddDbContext<NabizDbContext>(o => o.UseSqlServer(cfg.GetConnectionString("Nabiz"), sql => sql.EnableRetryOnFailure()));
        s.AddScoped<IUnitOfWork, UnitOfWork>();

        s.Configure<JwtOptions>(cfg.GetSection(JwtOptions.Bolum));
        s.Configure<PersonelOptions>(cfg.GetSection(PersonelOptions.Bolum));
        s.Configure<SmtpOptions>(cfg.GetSection(SmtpOptions.Bolum));
        s.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        s.AddSingleton<ITokenService, JwtTokenService>();
        s.AddSingleton<IZaman, SistemZamani>();
        s.AddSingleton<IUygulamaAyarlari, UygulamaAyarlari>();
        s.AddHttpClient();
        s.AddScoped<IEmailSender, SmtpEmailSender>();
        s.AddScoped<IWebhookGonderici, WebhookGonderici>();
        s.AddScoped<IPersonelKaynagi, SqlPersonelKaynagi>();
        s.AddHostedService<ArkaPlanIsleri>();
        return s;
    }
}
