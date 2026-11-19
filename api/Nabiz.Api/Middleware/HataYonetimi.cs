using Microsoft.AspNetCore.Mvc;
using Nabiz.Application.Exceptions;

namespace Nabiz.Api.Middleware;

/// <summary>Uygulama hatalarını uygun HTTP durum koduna çevirir; beklenmeyenleri loglayıp 500 döner.</summary>
public class HataYonetimi(RequestDelegate next, ILogger<HataYonetimi> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            var (kod, baslik) = ex switch
            {
                BulunamadiHatasi => (404, "Bulunamadı"),
                CakismaHatasi => (409, "Çakışma"),
                DogrulamaHatasi => (400, "Geçersiz istek"),
                YetkisizHatasi => (401, "Yetkisiz"),
                _ => (500, "Sunucu hatası"),
            };
            if (kod == 500) log.LogError(ex, "Beklenmeyen hata");
            if (ctx.Response.HasStarted) throw;
            ctx.Response.StatusCode = kod;
            await ctx.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = kod, Title = baslik,
                Detail = Mesajlar.Cevir(kod == 500 ? "Beklenmeyen bir hata oluştu." : ex.Message, Nabiz.Domain.Dil.Normalize(ctx.Request.Headers["X-Dil"].ToString())),
            });
        }
    }
}
