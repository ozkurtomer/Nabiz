using Nabiz.Application.Abstractions;
using Nabiz.Domain.Entities;

namespace Nabiz.Infrastructure.Persistence;

public class UnitOfWork(NabizDbContext db) : IUnitOfWork
{
    public IRepository<Kullanici> Kullanicilar { get; } = new Repository<Kullanici>(db);
    public IRepository<Soru> Sorular { get; } = new Repository<Soru>(db);
    public IRepository<SoruPaketi> Paketler { get; } = new Repository<SoruPaketi>(db);
    public IRepository<GonderimPlani> GonderimPlanlari { get; } = new Repository<GonderimPlani>(db);
    public IRepository<Gonderim> Gonderimler { get; } = new Repository<Gonderim>(db);
    public IRepository<GonderimPartisi> GonderimPartileri { get; } = new Repository<GonderimPartisi>(db);
    public IRepository<Cevap> Cevaplar { get; } = new Repository<Cevap>(db);
    public IRepository<Uyari> Uyarilar { get; } = new Repository<Uyari>(db);
    public IRepository<AyarDeger> Ayarlar { get; } = new Repository<AyarDeger>(db);
    public IRepository<DenetimKaydi> Denetim { get; } = new Repository<DenetimKaydi>(db);
    public IRepository<Aksiyon> Aksiyonlar { get; } = new Repository<Aksiyon>(db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
