using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

/// <summary>Bir uyarıdan ya da bulgudan doğan, sorumlusu ve tarihi olan iyileştirme görevi.</summary>
public class Aksiyon
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public string? Aciklama { get; set; }
    public AksiyonDurumu Durum { get; set; } = AksiyonDurumu.Acik;
    public AksiyonOnceligi Oncelik { get; set; } = AksiyonOnceligi.Orta;
    public int? SorumluKullaniciId { get; set; }
    public DateTime? SonTarih { get; set; }
    /// <summary>Aksiyonun doğduğu uyarı (uyarı silinirse boşalır).</summary>
    public int? UyariId { get; set; }
    /// <summary>Aksiyonun ilgili olduğu çalışan.</summary>
    public int? KullaniciId { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public DateTime Olusturma { get; set; }
    public DateTime? Kapanma { get; set; }
    /// <summary>Sonuç / ilerleme notu.</summary>
    public string? Not { get; set; }

    public bool Gecikmis(DateTime bugunYerel) =>
        SonTarih is not null && SonTarih.Value.Date < bugunYerel.Date && Durum is AksiyonDurumu.Acik or AksiyonDurumu.Devam;
}
