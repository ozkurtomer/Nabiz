using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public interface IDenetimService
{
    /// <summary>Kritik işlemi (kişi bazlı veriye bakma, gönderim, rol değişikliği…) kaydeder.</summary>
    Task KaydetAsync(string eylem, string? detay, CancellationToken ct);
    Task<List<DenetimDto>> ListeAsync(CancellationToken ct);
}

public class DenetimService(IUnitOfWork uow, ICurrentUser me, IZaman zaman) : IDenetimService
{
    public async Task KaydetAsync(string eylem, string? detay, CancellationToken ct)
    {
        var id = me.IdVeyaNull;
        var ad = id is null ? "Sistem" : (await uow.Kullanicilar.FindAsync(id, ct))?.Ad ?? "?";
        uow.Denetim.Add(new DenetimKaydi { Zaman = zaman.UtcNow, KullaniciId = id, KullaniciAd = ad, Eylem = eylem, Detay = detay });
        await uow.SaveChangesAsync(ct);
    }

    public async Task<List<DenetimDto>> ListeAsync(CancellationToken ct) =>
        (await uow.Denetim.Query.OrderByDescending(d => d.Zaman).Take(300).ToListAsync(ct))
            .Select(d => new DenetimDto(d.Id, zaman.Yerel(d.Zaman), d.KullaniciAd, d.Eylem, d.Detay)).ToList();
}
