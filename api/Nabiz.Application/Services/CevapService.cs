using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface ICevapService
{
    /// <summary>Cevap linkindeki gizli token tek başına yeterlidir (kişiye özel, tahmin edilemez); giriş gerekmez.</summary>
    Task<CevapSayfasiDto> GetirAsync(string token, CancellationToken ct);
    /// <summary>Cevapları kaydeder; Tamamla=true ise paketi kapatır (tüm sorular cevaplanmış olmalı) ve düşük puanlardan uyarı üretir.</summary>
    Task<CevapKaydetYanit> KaydetAsync(string token, CevapKaydetIstek istek, CancellationToken ct);
    /// <summary>Deneme: oturum açan kullanıcıya e-posta atmadan hemen bir paket gönderimi üretir.</summary>
    Task<TestGonderimYanit> TestGonderimiOlusturAsync(int kullaniciId, int paketId, CancellationToken ct);
    /// <summary>Kullanıcıya gelen son paketler ve tamamlanma durumları.</summary>
    Task<List<BenimSoruDto>> BenimAsync(int kullaniciId, CancellationToken ct);
}

public class CevapService(IUnitOfWork uow, IAyarService ayarlar, IZaman zaman, IUyariBildirimService bildirim, IDilBilgisi? dilBilgisi = null) : ICevapService
{
    private string Lang => dilBilgisi?.Dil ?? "tr";

    private IQueryable<Gonderim> Sorgu => uow.Gonderimler.Query
        .Include(g => g.Kullanici).Include(g => g.Cevaplar)
        .Include(g => g.Parti!).ThenInclude(p => p.Paket!).ThenInclude(p => p.Sorular).ThenInclude(s => s.Soru);

    private static List<Soru> PaketSorulari(Gonderim g) => g.Parti!.Paket!.Sorular.OrderBy(s => s.Sira).Select(s => s.Soru!).ToList();

    public async Task<CevapSayfasiDto> GetirAsync(string token, CancellationToken ct)
    {
        var g = await BulAsync(token, ct);
        var p = g.Parti!.Paket!;
        var a = await ayarlar.GetirAsync(ct);
        return new CevapSayfasiDto(g.Kullanici!.Ad, Nabiz.Domain.Dil.Normalize(g.Kullanici.Dil), p.Ad, p.AdEn, p.Aciklama, p.AciklamaEn, PaketSorulari(g).Select(SoruService.Dto).ToList(),
            g.Cevaplar.Select(c => new CevapGirdisi(c.SoruId, c.Deger, c.Yorum, c.YorumAnonim)).ToList(), g.Tamamlandi, a.EmojiEsik, a.PuanEsik);
    }

    public async Task<CevapKaydetYanit> KaydetAsync(string token, CevapKaydetIstek i, CancellationToken ct)
    {
        var g = await BulAsync(token, ct);
        if (g.Tamamlandi) throw new CakismaHatasi("Cevaplar tamamlandı, değiştirilemez.");
        var sorular = PaketSorulari(g).ToDictionary(s => s.Id);
        var dusukler = new List<(Soru Soru, Cevap Cevap)>();

        foreach (var girdi in i.Cevaplar)
        {
            if (!sorular.TryGetValue(girdi.SoruId, out var soru)) throw new DogrulamaHatasi("Bu soru pakette yok.");
            if (!soru.Degerler().Contains(girdi.Deger)) throw new DogrulamaHatasi("Geçersiz cevap değeri.");
            var c = g.Cevaplar.FirstOrDefault(x => x.SoruId == girdi.SoruId);
            if (c is null) { c = new Cevap { GonderimId = g.Id, SoruId = girdi.SoruId }; g.Cevaplar.Add(c); }
            c.Deger = girdi.Deger;
            c.Yorum = string.IsNullOrWhiteSpace(girdi.Yorum) ? null : girdi.Yorum.Trim();
            c.YorumAnonim = girdi.AnonimYorum && c.Yorum is not null;
            c.GuncellemeZamani = zaman.UtcNow;
        }

        if (i.Tamamla)
        {
            if (sorular.Keys.Any(id => g.Cevaplar.All(c => c.SoruId != id))) throw new DogrulamaHatasi("Tamamlamak için tüm soruları cevaplamalısın.");
            g.Tamamlandi = true; g.TamamlanmaZamani = zaman.UtcNow;
            var a = await ayarlar.GetirAsync(ct);
            dusukler = g.Cevaplar.Where(c => sorular[c.SoruId].DusukMu(c.Deger, a.EmojiEsik, a.PuanEsik)).Select(c => (sorular[c.SoruId], c)).ToList();
            foreach (var (_, c) in dusukler) uow.Uyarilar.Add(new Uyari { Cevap = c, Olusturma = zaman.UtcNow });
        }
        await uow.SaveChangesAsync(ct);
        if (dusukler.Count > 0) await bildirim.DusukPuanAsync(g, dusukler, ct);
        return new CevapKaydetYanit(g.Tamamlandi, g.Cevaplar.Count, sorular.Count);
    }

    public async Task<TestGonderimYanit> TestGonderimiOlusturAsync(int kullaniciId, int paketId, CancellationToken ct)
    {
        if (!await uow.Paketler.Query.AnyAsync(x => x.Id == paketId, ct)) throw new BulunamadiHatasi("Paket bulunamadı.");
        var parti = new GonderimPartisi
        {
            PaketId = paketId, HedefTipi = HedefTipi.Kisi, HedefDeger = kullaniciId.ToString(),
            Zaman = zaman.UtcNow, Anlik = true, Test = true, OlusturanKullaniciId = kullaniciId,
        };
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        uow.Gonderimler.Add(new Gonderim
        {
            Parti = parti, KullaniciId = kullaniciId, Token = token, GonderimZamani = zaman.UtcNow, Durum = GonderimDurumu.Gonderildi,
        });
        await uow.SaveChangesAsync(ct);
        return new TestGonderimYanit(token);
    }

    public async Task<List<BenimSoruDto>> BenimAsync(int kullaniciId, CancellationToken ct)
    {
        var liste = await uow.Gonderimler.Query.Include(g => g.Parti!).ThenInclude(p => p.Paket!).ThenInclude(p => p.Sorular)
            .Where(g => g.KullaniciId == kullaniciId && g.Durum == GonderimDurumu.Gonderildi)
            .OrderByDescending(g => g.GonderimZamani).Take(50).ToListAsync(ct);
        return liste.Select(g => new BenimSoruDto(g.Token, g.Parti!.Paket!.AdGoster(Lang), g.Parti.Paket.Sorular.Count, zaman.Yerel(g.GonderimZamani), g.Tamamlandi)).ToList();
    }

    private async Task<Gonderim> BulAsync(string token, CancellationToken ct) =>
        await Sorgu.FirstOrDefaultAsync(g => g.Token == token, ct) ?? throw new BulunamadiHatasi("Bu bağlantı geçersiz ya da süresi dolmuş.");
}
