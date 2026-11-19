using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IAksiyonService
{
    Task<List<AksiyonDto>> ListeAsync(AksiyonDurumu? durum, CancellationToken ct);
    Task<AksiyonOzetDto> OzetAsync(CancellationToken ct);
    Task<AksiyonDto> EkleAsync(AksiyonKaydetIstek istek, CancellationToken ct);
    Task<AksiyonDto> GuncelleAsync(int id, AksiyonKaydetIstek istek, CancellationToken ct);
    Task SilAsync(int id, CancellationToken ct);
}

/// <summary>Uyarılardan doğan iyileştirme görevlerini sorumlu, öncelik ve son tarihle takip eder.</summary>
public class AksiyonService(IUnitOfWork uow, IZaman zaman, ICurrentUser me, IDenetimService denetim) : IAksiyonService
{
    public async Task<List<AksiyonDto>> ListeAsync(AksiyonDurumu? durum, CancellationToken ct)
    {
        var q = uow.Aksiyonlar.Query;
        if (durum is not null) q = q.Where(a => a.Durum == durum);
        var liste = await q.OrderBy(a => a.Durum == AksiyonDurumu.Tamam || a.Durum == AksiyonDurumu.Iptal).ThenByDescending(a => a.Oncelik).ThenBy(a => a.SonTarih ?? DateTime.MaxValue).ThenByDescending(a => a.Id).Take(500).ToListAsync(ct);
        var adlar = await AdlariGetirAsync(liste, ct);
        return liste.Select(a => Dto(a, adlar)).ToList();
    }

    public async Task<AksiyonOzetDto> OzetAsync(CancellationToken ct)
    {
        var tumu = await uow.Aksiyonlar.Query.Select(a => new { a.Durum, a.SonTarih }).ToListAsync(ct);
        var bugun = zaman.Simdi.Date;
        return new AksiyonOzetDto(
            tumu.Count(a => a.Durum == AksiyonDurumu.Acik), tumu.Count(a => a.Durum == AksiyonDurumu.Devam), tumu.Count(a => a.Durum == AksiyonDurumu.Tamam),
            tumu.Count(a => a.SonTarih is not null && a.SonTarih.Value.Date < bugun && a.Durum is AksiyonDurumu.Acik or AksiyonDurumu.Devam));
    }

    public async Task<AksiyonDto> EkleAsync(AksiyonKaydetIstek i, CancellationToken ct)
    {
        var a = new Aksiyon { Olusturma = zaman.UtcNow, OlusturanKullaniciId = me.IdVeyaNull };
        await UygulaAsync(a, i, ct);
        uow.Aksiyonlar.Add(a);
        // Uyarıdan doğduysa uyarı "Takipte" olur: ilgilenildiği görünsün
        if (a.UyariId is int uid && await uow.Uyarilar.FindAsync(uid, ct) is { Durum: UyariDurumu.Yeni } uyari) uyari.Durum = UyariDurumu.Takipte;
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("Aksiyon oluşturuldu", a.Baslik, ct);
        return await GetirAsync(a.Id, ct);
    }

    public async Task<AksiyonDto> GuncelleAsync(int id, AksiyonKaydetIstek i, CancellationToken ct)
    {
        var a = await uow.Aksiyonlar.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        var eskiDurum = a.Durum;
        await UygulaAsync(a, i, ct);
        await uow.SaveChangesAsync(ct);
        if (eskiDurum != a.Durum) await denetim.KaydetAsync("Aksiyon durumu değişti", $"{a.Baslik}: {eskiDurum} → {a.Durum}", ct);
        return await GetirAsync(id, ct);
    }

    public async Task SilAsync(int id, CancellationToken ct)
    {
        var a = await uow.Aksiyonlar.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        uow.Aksiyonlar.Remove(a);
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("Aksiyon silindi", a.Baslik, ct);
    }

    private async Task UygulaAsync(Aksiyon a, AksiyonKaydetIstek i, CancellationToken ct)
    {
        var baslik = (i.Baslik ?? "").Trim();
        if (baslik.Length is 0 or > 200) throw new DogrulamaHatasi("Aksiyon başlığı 1-200 karakter olmalı.");
        if (i.SorumluId is int s && !await uow.Kullanicilar.Query.AnyAsync(k => k.Id == s, ct)) throw new DogrulamaHatasi("Sorumlu kullanıcı bulunamadı.");
        if (i.KullaniciId is int k2 && !await uow.Kullanicilar.Query.AnyAsync(k => k.Id == k2, ct)) throw new DogrulamaHatasi("İlgili çalışan bulunamadı.");
        a.Baslik = baslik; a.Aciklama = Bos(i.Aciklama); a.Not = Bos(i.Not);
        a.Oncelik = i.Oncelik; a.SorumluKullaniciId = i.SorumluId; a.SonTarih = i.SonTarih?.Date;
        a.UyariId ??= i.UyariId; a.KullaniciId ??= i.KullaniciId;
        // Kapanış zamanı: tamamlandı/iptal olunca damgalanır, yeniden açılınca temizlenir
        var kapali = i.Durum is AksiyonDurumu.Tamam or AksiyonDurumu.Iptal;
        if (kapali && a.Kapanma is null) a.Kapanma = zaman.UtcNow;
        if (!kapali) a.Kapanma = null;
        a.Durum = i.Durum;
    }

    private async Task<AksiyonDto> GetirAsync(int id, CancellationToken ct)
    {
        var a = await uow.Aksiyonlar.Query.FirstAsync(x => x.Id == id, ct);
        return Dto(a, await AdlariGetirAsync([a], ct));
    }

    private async Task<Dictionary<int, string>> AdlariGetirAsync(List<Aksiyon> liste, CancellationToken ct)
    {
        var ids = liste.SelectMany(a => new[] { a.SorumluKullaniciId, a.KullaniciId }).Where(x => x is not null).Select(x => x!.Value).Distinct().ToList();
        return ids.Count == 0 ? [] : await uow.Kullanicilar.Query.Where(k => ids.Contains(k.Id)).ToDictionaryAsync(k => k.Id, k => k.Ad, ct);
    }

    private AksiyonDto Dto(Aksiyon a, Dictionary<int, string> adlar) => new(
        a.Id, a.Baslik, a.Aciklama, a.Durum, a.Oncelik, a.SorumluKullaniciId, a.SorumluKullaniciId is int s ? adlar.GetValueOrDefault(s) : null,
        a.SonTarih, a.Gecikmis(zaman.Simdi), a.UyariId, a.KullaniciId, a.KullaniciId is int k ? adlar.GetValueOrDefault(k) : null,
        zaman.Yerel(a.Olusturma), a.Kapanma is null ? null : zaman.Yerel(a.Kapanma.Value), a.Not);

    private static string? Bos(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}
