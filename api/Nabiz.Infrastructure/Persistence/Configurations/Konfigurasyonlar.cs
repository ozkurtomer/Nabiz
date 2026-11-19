using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nabiz.Domain.Entities;

namespace Nabiz.Infrastructure.Persistence.Configurations;

public class KullaniciConfig : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> b)
    {
        b.Property(x => x.PersonelId).HasMaxLength(100).IsRequired();
        b.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200).IsRequired();
        b.Property(x => x.Departman).HasMaxLength(200);
        b.Property(x => x.YoneticiPersonelId).HasMaxLength(100);
        b.Property(x => x.SifreHash).HasMaxLength(100).IsRequired();
        b.Property(x => x.Dil).HasMaxLength(5).IsRequired();
        b.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        b.Ignore(x => x.SifreVar);
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.PersonelId).IsUnique();
    }
}

public class SoruConfig : IEntityTypeConfiguration<Soru>
{
    public void Configure(EntityTypeBuilder<Soru> b)
    {
        b.Property(x => x.MetinTr).HasMaxLength(500).IsRequired();
        b.Property(x => x.MetinEn).HasMaxLength(500).IsRequired();
        b.Property(x => x.Tip).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.TakipMetniTr).HasMaxLength(300);
        b.Property(x => x.TakipMetniEn).HasMaxLength(300);
        b.HasData(SoruSeed.Sorular);
    }
}

public class SoruPaketiConfig : IEntityTypeConfiguration<SoruPaketi>
{
    public void Configure(EntityTypeBuilder<SoruPaketi> b)
    {
        b.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(1000);
        b.HasMany(x => x.Sorular).WithOne().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Cascade);
        b.HasData(SoruSeed.Paketler);
    }
}

public class PaketSoruConfig : IEntityTypeConfiguration<PaketSoru>
{
    public void Configure(EntityTypeBuilder<PaketSoru> b)
    {
        b.HasKey(x => new { x.PaketId, x.SoruId });
        b.HasOne(x => x.Soru).WithMany().HasForeignKey(x => x.SoruId).OnDelete(DeleteBehavior.Restrict);
        b.HasData(SoruSeed.PaketSorulari);
    }
}

public class GonderimPlaniConfig : IEntityTypeConfiguration<GonderimPlani>
{
    public void Configure(EntityTypeBuilder<GonderimPlani> b)
    {
        b.Property(x => x.HedefTipi).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.HedefDeger).HasMaxLength(2000);
        b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GonderimPartisiConfig : IEntityTypeConfiguration<GonderimPartisi>
{
    public void Configure(EntityTypeBuilder<GonderimPartisi> b)
    {
        b.Property(x => x.HedefTipi).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.HedefDeger).HasMaxLength(2000);
        b.HasOne(x => x.Paket).WithMany().HasForeignKey(x => x.PaketId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Zaman);
    }
}

public class GonderimConfig : IEntityTypeConfiguration<Gonderim>
{
    public void Configure(EntityTypeBuilder<Gonderim> b)
    {
        b.Property(x => x.Token).HasMaxLength(64).IsRequired();
        b.Property(x => x.Durum).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Hata).HasMaxLength(500);
        b.HasIndex(x => x.Token).IsUnique();
        b.HasOne(x => x.Parti).WithMany().HasForeignKey(x => x.PartiId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Kullanici).WithMany().HasForeignKey(x => x.KullaniciId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Cevaplar).WithOne().HasForeignKey(x => x.GonderimId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.GonderimZamani);
    }
}

public class CevapConfig : IEntityTypeConfiguration<Cevap>
{
    public void Configure(EntityTypeBuilder<Cevap> b)
    {
        b.Property(x => x.Yorum).HasMaxLength(2000);
        b.HasOne(x => x.Soru).WithMany().HasForeignKey(x => x.SoruId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.GonderimId, x.SoruId }).IsUnique();
    }
}

public class UyariConfig : IEntityTypeConfiguration<Uyari>
{
    public void Configure(EntityTypeBuilder<Uyari> b)
    {
        b.Property(x => x.Durum).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.IkNotu).HasMaxLength(2000);
        b.HasOne(x => x.Cevap).WithMany().HasForeignKey(x => x.CevapId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.CevapId).IsUnique();
    }
}

public class AyarDegerConfig : IEntityTypeConfiguration<AyarDeger>
{
    public void Configure(EntityTypeBuilder<AyarDeger> b)
    {
        b.HasKey(x => x.Anahtar);
        b.Property(x => x.Anahtar).HasMaxLength(100);
        b.Property(x => x.Deger).HasMaxLength(500).IsRequired();
    }
}

public class DenetimKaydiConfig : IEntityTypeConfiguration<DenetimKaydi>
{
    public void Configure(EntityTypeBuilder<DenetimKaydi> b)
    {
        b.Property(x => x.KullaniciAd).HasMaxLength(200).IsRequired();
        b.Property(x => x.Eylem).HasMaxLength(200).IsRequired();
        b.Property(x => x.Detay).HasMaxLength(1000);
        b.HasIndex(x => x.Zaman);
    }
}
