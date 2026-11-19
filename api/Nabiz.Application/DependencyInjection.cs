using Nabiz.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Nabiz.Application.Services;

namespace Nabiz.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection s)
    {
        s.AddScoped<IAuthService, AuthService>();
        s.AddScoped<ISoruService, SoruService>();
        s.AddScoped<IKullaniciService, KullaniciService>();
        s.AddScoped<IPaketService, PaketService>();
        s.AddScoped<IPlanService, PlanService>();
        s.AddScoped<IAyarService, AyarService>();
        s.AddScoped<ICevapService, CevapService>();
        s.AddScoped<IUyariService, UyariService>();
        s.AddScoped<IGonderimService, GonderimService>();
        s.AddScoped<IRaporService, RaporService>();
        s.AddScoped<IDemoVeriService, DemoVeriService>();
        s.AddScoped<IDenetimService, DenetimService>();
        s.AddScoped<IUyariBildirimService, UyariBildirimService>();
        s.AddScoped<ISistemService, SistemService>();
        s.AddScoped<IAyarDeposu, AyarDeposu>();
        s.AddScoped<IMarkaService, MarkaService>();
        s.AddScoped<IAksiyonService, AksiyonService>();
        s.AddScoped<IKvkkService, KvkkService>();
        return s;
    }
}
