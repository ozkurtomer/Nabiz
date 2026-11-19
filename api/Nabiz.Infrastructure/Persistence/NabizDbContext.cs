using Microsoft.EntityFrameworkCore;
using Nabiz.Domain.Entities;

namespace Nabiz.Infrastructure.Persistence;

public class NabizDbContext(DbContextOptions<NabizDbContext> options) : DbContext(options)
{
    public const string Sema = "nabiz";

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Soru> Sorular => Set<Soru>();
    public DbSet<SoruPaketi> Paketler => Set<SoruPaketi>();
    public DbSet<GonderimPlani> GonderimPlanlari => Set<GonderimPlani>();
    public DbSet<Gonderim> Gonderimler => Set<Gonderim>();
    public DbSet<GonderimPartisi> GonderimPartileri => Set<GonderimPartisi>();
    public DbSet<Cevap> Cevaplar => Set<Cevap>();
    public DbSet<Uyari> Uyarilar => Set<Uyari>();
    public DbSet<AyarDeger> Ayarlar => Set<AyarDeger>();
    public DbSet<DenetimKaydi> Denetim => Set<DenetimKaydi>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Uygulamanın tabloları ayrı şemada; personel tablosuna dokunulmaz.
        b.HasDefaultSchema(Sema);
        b.ApplyConfigurationsFromAssembly(typeof(NabizDbContext).Assembly);
    }
}
