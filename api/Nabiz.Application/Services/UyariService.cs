using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;

namespace Nabiz.Application.Services;

public interface IUyariService
{
    Task<List<UyariDto>> ListeAsync(CancellationToken ct);
    Task GuncelleAsync(int id, UyariGuncelleIstek istek, CancellationToken ct);
}

public class UyariService(IUnitOfWork uow, IZaman zaman) : IUyariService
{
    public async Task<List<UyariDto>> ListeAsync(CancellationToken ct)
    {
        var q = from u in uow.Uyarilar.Query
                join c in uow.Cevaplar.Query on u.CevapId equals c.Id
                join g in uow.Gonderimler.Query on c.GonderimId equals g.Id
                join k in uow.Kullanicilar.Query on g.KullaniciId equals k.Id
                join s in uow.Sorular.Query on c.SoruId equals s.Id
                orderby u.Olusturma descending
                select new { u.Id, u.Olusturma, u.Durum, u.IkNotu, KullaniciId = k.Id, k.Ad, k.Departman, Soru = s.MetinTr, c.Deger, c.Yorum, c.YorumAnonim };
        var satirlar = await q.Take(300).ToListAsync(ct);

        // "Son 30 günde kaç uyarı": üst üste düşük puan veren kişileri öne çıkarmak için
        var sinir = zaman.UtcNow.AddDays(-30);
        var sayilar = satirlar.Where(x => x.Olusturma >= sinir).GroupBy(x => x.KullaniciId).ToDictionary(g => g.Key, g => g.Count());
        return satirlar.Select(x => new UyariDto(x.Id, x.Olusturma, x.Durum, x.IkNotu, x.KullaniciId, x.Ad, x.Departman, x.Soru, x.Deger,
            x.YorumAnonim ? null : x.Yorum, x.YorumAnonim && !string.IsNullOrWhiteSpace(x.Yorum), sayilar.GetValueOrDefault(x.KullaniciId))).ToList();
    }

    public async Task GuncelleAsync(int id, UyariGuncelleIstek i, CancellationToken ct)
    {
        var u = await uow.Uyarilar.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        u.Durum = i.Durum; u.IkNotu = i.IkNotu;
        await uow.SaveChangesAsync(ct);
    }
}
