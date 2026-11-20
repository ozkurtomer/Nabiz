using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Application.Exceptions;
using Nabiz.Domain.Entities;

namespace Nabiz.Application.Services;

public interface IAuthService
{
    /// <summary>E-posta + şifre (yalnızca şifresi olan hesaplar: ilk İK hesabı, demo).</summary>
    Task<LoginYanit> LoginAsync(LoginIstek istek, CancellationToken ct);
    /// <summary>Şifresiz giriş: e-postaya tek kullanımlık (15 dk) giriş linki gönderir. Hesap yoksa da sessizce başarılı döner.</summary>
    Task GirisLinkiGonderAsync(string email, CancellationToken ct);
    /// <summary>Mobil uygulamanın backend'i için: paylaşılan gizli anahtarla kullanıcı adına tek kullanımlık WebView giriş adresi üretir.</summary>
    Task<MobilGirisYanit> MobilGirisAsync(string email, string? anahtar, CancellationToken ct);
    /// <summary>E-postadaki giriş linkinin token'ını oturuma çevirir.</summary>
    Task<LoginYanit> GirisLinkiDogrulaAsync(string token, CancellationToken ct);
}

public class AuthService(IUnitOfWork uow, IPasswordHasher hasher, ITokenService tokens, IEmailSender mail, IUygulamaAyarlari uygulama) : IAuthService
{
    // Brute-force koruması: bir e-posta için 15 dakikada 5 hatalı denemeden sonra giriş geçici kilitlenir (bellek içi).
    private const int MaksDeneme = 5;
    private static readonly TimeSpan Pencere = TimeSpan.FromMinutes(15);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (int Sayi, DateTime Ilk)> Denemeler = new();

    public async Task<LoginYanit> LoginAsync(LoginIstek istek, CancellationToken ct)
    {
        var anahtar = (istek.Email ?? "").Trim().ToLowerInvariant();
        if (Denemeler.TryGetValue(anahtar, out var d) && DateTime.UtcNow - d.Ilk < Pencere && d.Sayi >= MaksDeneme)
            throw new YetkisizHatasi("Çok fazla hatalı deneme. Lütfen 15 dakika sonra tekrar dene.");
        var k = await uow.Kullanicilar.Query.FirstOrDefaultAsync(x => x.Email == istek.Email && x.Aktif, ct);
        if (k is null || !k.SifreVar || !hasher.Dogrula(istek.Sifre, k.SifreHash))
        {
            Denemeler.AddOrUpdate(anahtar, (1, DateTime.UtcNow),
                (_, o) => DateTime.UtcNow - o.Ilk >= Pencere ? (1, DateTime.UtcNow) : (o.Sayi + 1, o.Ilk));
            throw new YetkisizHatasi("E-posta veya şifre hatalı.");
        }
        Denemeler.TryRemove(anahtar, out _);
        return Yanit(k);
    }

    public async Task GirisLinkiGonderAsync(string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DogrulamaHatasi("E-posta zorunlu.");
        var k = await uow.Kullanicilar.Query.FirstOrDefaultAsync(x => x.Email == email.Trim() && x.Aktif, ct);
        if (k is null) return; // hesap var mı yok mu belli etme
        var link = $"{uygulama.PublicUrl.TrimEnd('/')}/giris/dogrula?t={Uri.EscapeDataString(tokens.GirisLinkiTokeniOlustur(k))}";
        var (konu, html) = EmailSablonu.GirisLinki(k, link);
        await mail.GonderAsync(k.Email, konu, html, ct);
    }

    public async Task<MobilGirisYanit> MobilGirisAsync(string email, string? anahtar, CancellationToken ct)
    {
        var beklenen = uygulama.MobilAnahtar;
        if (string.IsNullOrWhiteSpace(beklenen) || string.IsNullOrEmpty(anahtar)
            || !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(beklenen), System.Text.Encoding.UTF8.GetBytes(anahtar)))
            throw new YetkisizHatasi("Geçersiz mobil anahtar.");
        var k = await uow.Kullanicilar.Query.FirstOrDefaultAsync(x => x.Email == email.Trim() && x.Aktif, ct) ?? throw new BulunamadiHatasi("Kullanıcı bulunamadı.");
        var token = tokens.GirisLinkiTokeniOlustur(k);
        return new MobilGirisYanit($"{uygulama.PublicUrl.TrimEnd('/')}/giris/dogrula?t={Uri.EscapeDataString(token)}", token);
    }

    public async Task<LoginYanit> GirisLinkiDogrulaAsync(string token, CancellationToken ct)
    {
        var id = await tokens.GirisLinkiTokeniDogrulaAsync(token) ?? throw new YetkisizHatasi("Giriş linki geçersiz ya da süresi dolmuş.");
        var k = await uow.Kullanicilar.FindAsync(id, ct);
        if (k is null || !k.Aktif) throw new YetkisizHatasi("Hesap bulunamadı.");
        return Yanit(k);
    }

    private LoginYanit Yanit(Kullanici k) => new(tokens.Olustur(k), k.Rol.ToString(), k.Dil, k.Ad);
}
