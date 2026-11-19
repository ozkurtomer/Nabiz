using System.Security.Claims;
using Nabiz.Application.Abstractions;

namespace Nabiz.Api.Security;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Nabiz.Domain.Enums.Rol Rol => Enum.TryParse<Nabiz.Domain.Enums.Rol>(
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role), out var r) ? r : Nabiz.Domain.Enums.Rol.Calisan;

    public int? IdVeyaNull => int.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public int Id => int.Parse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Oturum bulunamadı."));
}

public static class Roller
{
    /// <summary>İnsan Kaynakları: raporlar, gönderim, paket/soru/kullanıcı yönetimi, ayarlar ve denetim.</summary>
    public const string IK = "IK";
}
