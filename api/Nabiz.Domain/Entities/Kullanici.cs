using Nabiz.Domain.Enums;

namespace Nabiz.Domain.Entities;

public class Kullanici
{
    /// <summary>İlk açılışta oluşturulan İK hesabının sabit sicili; personel senkronu bu hesabı pasifleştirmez.</summary>
    public const string IlkPersonelId = "ILK-IK";

    public int Id { get; set; }
    /// <summary>Mevcut personel tablosundaki kayıt anahtarı (sicil).</summary>
    public string PersonelId { get; set; } = "";
    public string Ad { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Departman { get; set; }
    public string SifreHash { get; set; } = "";
    public Rol Rol { get; set; } = Rol.Calisan;
    public string Dil { get; set; } = "tr";
    public bool Aktif { get; set; } = true;

    public bool SifreVar => !string.IsNullOrEmpty(SifreHash);
}
