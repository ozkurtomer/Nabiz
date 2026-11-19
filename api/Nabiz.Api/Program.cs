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

var app = builder.Build();

await BaslangicVerisi.UygulaAsync(app.Services, app.Configuration);

app.UseMiddleware<HataYonetimi>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/saglik", () => Results.Ok(new { durum = "ok" }));

app.Run();
