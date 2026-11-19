using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IKullaniciService
{
    Task<List<KullaniciDto>> ListeAsync(string? ara, CancellationToken ct);
    Task GuncelleAsync(int id, KullaniciGuncelleIstek istek, CancellationToken ct);
    Task<PersonelAktarimSonucu> PersonelAktarAsync(CancellationToken ct);
    /// <summary>Kullanıcının kendi arayüz/e-posta dilini ("tr"/"en") kaydeder.</summary>
    Task DilDegistirAsync(int kullaniciId, string dil, CancellationToken ct);
}

public class KullaniciService(IUnitOfWork uow, IPersonelKaynagi personel, IDenetimService denetim, IZaman zaman) : IKullaniciService
{
    private static KullaniciDto Dto(Kullanici k) =>
        new(k.Id, k.PersonelId, k.Ad, k.Email, k.Departman, k.Rol, k.Dil, k.Aktif);

    public async Task<List<KullaniciDto>> ListeAsync(string? ara, CancellationToken ct)
    {
        var q = uow.Kullanicilar.Query.AsQueryable();
        if (!string.IsNullOrWhiteSpace(ara)) q = q.Where(x => x.Ad.Contains(ara) || x.Email.Contains(ara));
        var liste = await q.OrderBy(x => x.Ad).ToListAsync(ct);
        return liste.Select(Dto).ToList();
    }

    public async Task GuncelleAsync(int id, KullaniciGuncelleIstek i, CancellationToken ct)
    {
        var k = await uow.Kullanicilar.FindAsync(id, ct) ?? throw new BulunamadiHatasi();
        var eski = $"{k.Rol}/{(k.Aktif ? "active" : "inactive")}";
        k.Rol = i.Rol; k.Aktif = i.Aktif; k.Dil = Nabiz.Domain.Dil.Normalize(i.Dil);
        await uow.SaveChangesAsync(ct);
        var yeni = $"{k.Rol}/{(k.Aktif ? "active" : "inactive")}";
        if (eski != yeni) await denetim.KaydetAsync("Kullanıcı rolü/erişimi değişti", $"{k.Ad}: {eski} → {yeni}", ct);
    }

    public async Task DilDegistirAsync(int kullaniciId, string dil, CancellationToken ct)
    {
        var k = await uow.Kullanicilar.FindAsync(kullaniciId, ct) ?? throw new BulunamadiHatasi("Kullanıcı bulunamadı.");
        k.Dil = Nabiz.Domain.Dil.Normalize(dil);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<PersonelAktarimSonucu> PersonelAktarAsync(CancellationToken ct)
    {
        var satirlar = await personel.OkuAsync(ct);
        var mevcut = await uow.Kullanicilar.Query.ToDictionaryAsync(x => x.PersonelId, ct);
        var epostalar = mevcut.Values.Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        int yeni = 0, guncel = 0, atlanan = 0;
        var gorulen = new HashSet<string>();

        foreach (var s in satirlar)
        {
            if (string.IsNullOrWhiteSpace(s.Email)) { atlanan++; continue; }
            gorulen.Add(s.PersonelId);
            if (mevcut.TryGetValue(s.PersonelId, out var k))
            {
                k.Ad = s.Ad; k.Departman = s.Departman; k.Aktif = true;
                if (!string.Equals(k.Email, s.Email, StringComparison.OrdinalIgnoreCase) && !epostalar.Contains(s.Email)) k.Email = s.Email;
                guncel++;
            }
            else if (epostalar.Contains(s.Email)) { atlanan++; }
            else
            {
                uow.Kullanicilar.Add(new Kullanici
                {
                    PersonelId = s.PersonelId, Ad = s.Ad, Email = s.Email, Departman = s.Departman,
                    Rol = Rol.Calisan,
                });
                epostalar.Add(s.Email);
                yeni++;
            }
        }
        // Personel kaynağında artık bulunmayanları pasifleştir (kaynak boş dönerse hiçbir şey yapma).
        if (gorulen.Count > 0)
            foreach (var k in mevcut.Values.Where(k => k.Aktif && k.PersonelId != Kullanici.IlkPersonelId && !gorulen.Contains(k.PersonelId)))
                k.Aktif = false;
        var senkron = await uow.Ayarlar.FindAsync(SistemService.SonSenkronAnahtari, ct);
        var simdi = zaman.UtcNow.ToString("o");
        if (senkron is null) uow.Ayarlar.Add(new Domain.Entities.AyarDeger { Anahtar = SistemService.SonSenkronAnahtari, Deger = simdi }); else senkron.Deger = simdi;
        await uow.SaveChangesAsync(ct);
        return new PersonelAktarimSonucu(yeni, guncel, atlanan);
    }
}
