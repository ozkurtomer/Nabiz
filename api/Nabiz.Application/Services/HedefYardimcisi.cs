using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Exceptions;
using Nabiz.Domain;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

/// <summary>Hedef kitle (Herkes / Departman / Kişi) için ortak doğrulama, kullanıcı seçimi ve açıklama.</summary>
public static class HedefYardimcisi
{
    public static async Task DogrulaAsync(IUnitOfWork uow, HedefTipi tip, string? deger, CancellationToken ct)
    {
        switch (tip)
        {
            case HedefTipi.Departman when string.IsNullOrWhiteSpace(deger):
                throw new DogrulamaHatasi("Departman seçilmeli.");
            case HedefTipi.Kisi:
                var ids = Hedef.KisiIdleri(deger);
                if (ids.Count == 0) throw new DogrulamaHatasi("En az bir kişi seçilmeli.");
                var bulunan = await uow.Kullanicilar.Query.CountAsync(k => ids.Contains(k.Id), ct);
                if (bulunan != ids.Count) throw new DogrulamaHatasi("Seçilen kişilerden bazıları bulunamadı.");
                break;
        }
    }

    public static async Task<List<Nabiz.Domain.Entities.Kullanici>> KullanicilariGetirAsync(IUnitOfWork uow, HedefTipi tip, string? deger, CancellationToken ct)
    {
        var q = uow.Kullanicilar.Query.Where(k => k.Aktif);
        if (tip == HedefTipi.Departman) q = q.Where(k => k.Departman == deger);
        else if (tip == HedefTipi.Kisi) { var ids = Hedef.KisiIdleri(deger); q = q.Where(k => ids.Contains(k.Id)); }
        return await q.ToListAsync(ct);
    }

    public static string Aciklama(HedefTipi tip, string? deger, IReadOnlyDictionary<int, string> isimler)
    {
        switch (tip)
        {
            case HedefTipi.Hepsi: return "Tüm çalışanlar";
            case HedefTipi.Departman: return $"Departman: {deger}";
            default:
                var adlar = Hedef.KisiIdleri(deger).Select(i => isimler.TryGetValue(i, out var a) ? a : "?").ToList();
                return adlar.Count <= 3 ? string.Join(", ", adlar) : $"{adlar.Count} kişi: {string.Join(", ", adlar.Take(3))}…";
        }
    }
}
