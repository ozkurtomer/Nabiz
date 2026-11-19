using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Nabiz.Api.Extensions;
using Nabiz.Api.Middleware;
using Nabiz.Api.Security;
using Nabiz.Application;
using Nabiz.Application.Abstractions;
using Nabiz.Infrastructure;
using Nabiz.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IDilBilgisi, DilBilgisi>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtKimlikDogrulama(builder.Configuration);
builder.Services.AddCorsAyarlari(builder.Configuration);

// Kaba kuvvet ve istismara karşı: kimlik uçlarında IP başına dakikada 10, genel API'de 300 istek
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("kimlik", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "?", _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "?", _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear(); o.KnownProxies.Clear();   // proxy (nginx) aynı özel ağda; gerçek istemci IP'si hız sınırı için gerekir
});

var app = builder.Build();

await BaslangicVerisi.UygulaAsync(app.Services, app.Configuration);

app.UseForwardedHeaders();
app.UseMiddleware<HataYonetimi>();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/saglik", () => Results.Ok(new { durum = "ok" }));

app.Run();
