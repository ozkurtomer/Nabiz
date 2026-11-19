using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nabiz.Application.Abstractions;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;
using Nabiz.Infrastructure.Persistence;

namespace Nabiz.Infrastructure.Seed;

public static class BaslangicVerisi
{
    /// <summary>Migration'ları uygular, ilk admini ve (isteğe bağlı) demo verisini oluşturur.</summary>
    public static async Task UygulaAsync(IServiceProvider sp, IConfiguration cfg)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NabizDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Baslangic");

        if (cfg.GetValue<bool>("Baslangic:OtomatikMigration"))
        {
            await MigrationUygulaAsync(db, log);
        }

        if (cfg.GetValue<bool>("Baslangic:DemoVeri"))
            await DemoPersonelTablosuAsync(db);

        var admin = cfg.GetSection("Baslangic:IlkAdmin");
        if (!await db.Kullanicilar.AnyAsync() && !string.IsNullOrWhiteSpace(admin["Email"]) && !string.IsNullOrWhiteSpace(admin["Sifre"]))
        {
            db.Kullanicilar.Add(YeniKullanici("ADMIN", "Sistem Yöneticisi", admin["Email"]!, "BT", Rol.Admin, admin["Sifre"]!, hasher));
            await db.SaveChangesAsync();
            log.LogInformation("İlk admin oluşturuldu: {Email}", admin["Email"]);
        }

        if (cfg.GetValue<bool>("Baslangic:DemoVeri") && !await db.Kullanicilar.AnyAsync(x => x.PersonelId != "ADMIN"))
        {
            // Demo kullanıcıların hepsinin şifresi: Demo123!
            db.Kullanicilar.AddRange(
                YeniKullanici("1001", "Ayşe İK", "ik@nabiz.local", "İnsan Kaynakları", Rol.IK, "Demo123!", hasher),
                YeniKullanici("1002", "Mehmet Yönetici", "yonetici@nabiz.local", "Yazılım", Rol.Yonetici, "Demo123!", hasher),
                YeniKullanici("1003", "Elif Çalışan", "calisan@nabiz.local", "Yazılım", Rol.Calisan, "Demo123!", hasher));
            await db.SaveChangesAsync();
            log.LogInformation("Demo kullanıcılar oluşturuldu (şifre: Demo123!).");
        }
    }

    private static Kullanici YeniKullanici(string sicil, string ad, string email, string dep, Rol rol, string sifre, IPasswordHasher h) =>
        new() { PersonelId = sicil, Ad = ad, Email = email, Departman = dep, Rol = rol, SifreHash = h.Hashle(sifre) };

    /// <summary>SQL Server henüz hazır değilse (ör. konteyner açılışı) birkaç kez yeniden dener; veritabanı yoksa oluşturur.</summary>
    private static async Task MigrationUygulaAsync(NabizDbContext db, ILogger log)
    {
        for (var deneme = 1; ; deneme++)
        {
            try { await db.Database.MigrateAsync(); return; }
            catch (Exception ex) when (deneme < 30)
            {
                log.LogWarning("Migration denemesi {Deneme}/30 başarısız, bekleniyor: {Mesaj}", deneme, ex.GetBaseException().Message);
                await Task.Delay(3000);
            }
        }
    }

    /// <summary>Yalnızca demo ortamı: gerçek personel tablosunu taklit eden dbo.Personel oluşturur.</summary>
    private static async Task DemoPersonelTablosuAsync(NabizDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.Personel') IS NULL
BEGIN
    CREATE TABLE dbo.Personel (Sicil nvarchar(50) PRIMARY KEY, AdSoyad nvarchar(200), Email nvarchar(200), Departman nvarchar(200), Yonetici nvarchar(50), Aktif bit);
    INSERT INTO dbo.Personel VALUES
     ('1001', N'Ayşe İK',          'ik@nabiz.local',       N'İnsan Kaynakları', NULL,   1),
     ('1002', N'Mehmet Yönetici',  'yonetici@nabiz.local', N'Yazılım',          NULL,   1),
     ('1003', N'Elif Çalışan',     'calisan@nabiz.local',  N'Yazılım',          '1002', 1),
     ('2001', N'Can Yılmaz',   'can@nabiz.local',   N'Yazılım', '1002', 1),
     ('2002', N'Deniz Kaya',   'deniz@nabiz.local', N'Yazılım', '1002', 1),
     ('2003', N'Zeynep Aras',  'zeynep@nabiz.local',N'Satış',   NULL,   1),
     ('2004', N'Burak Demir',  'burak@nabiz.local', N'Satış',   NULL,   1),
     ('2005', N'Eski Çalışan', 'eski@nabiz.local',  N'Satış',   NULL,   0);
END");
    }
}
