namespace Nabiz.Application.Abstractions;

/// <summary>Anahtar/değer ayarları (marka, KVKK, zamanlayıcı kayıtları). Yazma, çağıranın SaveChanges'ine kadar bekler.</summary>
public interface IAyarDeposu
{
    Task<string?> OkuAsync(string anahtar, CancellationToken ct);
    Task YazAsync(string anahtar, string deger, CancellationToken ct);
}
