using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Exceptions;
using Nabiz.Infrastructure.Persistence;

namespace Nabiz.Infrastructure.Personel;

public class PersonelOptions
{
    public const string Bolum = "Personel";
    public string Tablo { get; set; } = "dbo.Personel";
    public string SicilKolon { get; set; } = "Sicil";
    public string AdKolon { get; set; } = "AdSoyad";
    public string EmailKolon { get; set; } = "Email";
    public string? DepartmanKolon { get; set; }
    public string? YoneticiKolon { get; set; }
    /// <summary>Boş değilse bu bit sütunu 1 olan personel alınır (ör. "Aktif").</summary>
    public string? AktifKolon { get; set; }
}

public class PersonelSatiri
{
    public string PersonelId { get; set; } = "";
    public string Ad { get; set; } = "";
    public string? Email { get; set; }
    public string? Departman { get; set; }
    public string? YoneticiPersonelId { get; set; }
}

/// <summary>Personel tablosunu SALT OKUNUR okur; tablo/sütun adları yapılandırmadan gelir.</summary>
public partial class SqlPersonelKaynagi(NabizDbContext db, IOptions<PersonelOptions> options) : IPersonelKaynagi
{
    [GeneratedRegex(@"^[\p{L}0-9_]+(\.[\p{L}0-9_]+)?$")]
    private static partial Regex GuvenliAd();

    private readonly PersonelOptions _o = options.Value;

    public async Task<IReadOnlyList<PersonelKaydi>> OkuAsync(CancellationToken ct = default)
    {
        var sql = $@"SELECT CAST({Id(_o.SicilKolon)} AS nvarchar(100)) AS PersonelId,
                            CAST({Id(_o.AdKolon)} AS nvarchar(200)) AS Ad,
                            CAST({Id(_o.EmailKolon)} AS nvarchar(200)) AS Email,
                            {Opsiyonel(_o.DepartmanKolon, 200)} AS Departman,
                            {Opsiyonel(_o.YoneticiKolon, 100)} AS YoneticiPersonelId
                     FROM {Id(_o.Tablo)}
                     {(string.IsNullOrWhiteSpace(_o.AktifKolon) ? "" : $"WHERE {Id(_o.AktifKolon)} = 1")}";

        var satirlar = await db.Database.SqlQueryRaw<PersonelSatiri>(sql).ToListAsync(ct);
        return satirlar.Select(s => new PersonelKaydi(s.PersonelId, s.Ad, s.Email, s.Departman, s.YoneticiPersonelId)).ToList();
    }

    private static string Opsiyonel(string? kolon, int uzunluk) =>
        string.IsNullOrWhiteSpace(kolon) ? $"CAST(NULL AS nvarchar({uzunluk}))" : $"CAST({Id(kolon)} AS nvarchar({uzunluk}))";

    /// <summary>Yapılandırmadan gelen tablo/sütun adını doğrular ve [köşeli] biçime çevirir.</summary>
    private static string Id(string ad)
    {
        if (string.IsNullOrWhiteSpace(ad) || !GuvenliAd().IsMatch(ad))
            throw new DogrulamaHatasi($"Personel ayarı geçersiz: '{ad}'.");
        return string.Join('.', ad.Split('.').Select(p => $"[{p}]"));
    }
}
