using Nabiz.Domain.Entities;

namespace Nabiz.Application.Abstractions;

public interface IRepository<T> where T : class
{
    /// <summary>Sorgulanabilir kaynak (LINQ / Include için).</summary>
    IQueryable<T> Query { get; }
    Task<T?> FindAsync(object id, CancellationToken ct = default);
    void Add(T entity);
    void AddRange(params T[] entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}

/// <summary>Tüm repository'leri tek bir SaveChanges (tek transaction) altında toplar.</summary>
public interface IUnitOfWork
{
    IRepository<Kullanici> Kullanicilar { get; }
    IRepository<Soru> Sorular { get; }
    IRepository<SoruPaketi> Paketler { get; }
    IRepository<GonderimPlani> GonderimPlanlari { get; }
    IRepository<Gonderim> Gonderimler { get; }
    IRepository<GonderimPartisi> GonderimPartileri { get; }
    IRepository<Cevap> Cevaplar { get; }
    IRepository<Uyari> Uyarilar { get; }
    IRepository<AyarDeger> Ayarlar { get; }
    IRepository<DenetimKaydi> Denetim { get; }
    IRepository<Aksiyon> Aksiyonlar { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
