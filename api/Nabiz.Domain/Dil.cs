namespace Nabiz.Domain;

/// <summary>Desteklenen diller ("tr", "en") ve çok dilli metin seçimi.</summary>
public static class Dil
{
    public static string Normalize(string? dil) => string.Equals(dil?.Trim(), "en", StringComparison.OrdinalIgnoreCase) ? "en" : "tr";

    /// <summary>İstenen dildeki metni verir; o dilde metin yoksa diğer dile düşer (iki dilden yalnızca biri zorunludur).</summary>
    public static string Sec(string? dil, string? tr, string? en)
    {
        var ingilizce = Normalize(dil) == "en";
        var asil = ingilizce ? en : tr;
        var yedek = ingilizce ? tr : en;
        return !string.IsNullOrWhiteSpace(asil) ? asil! : yedek ?? "";
    }

    public static string? SecOpsiyonel(string? dil, string? tr, string? en)
    {
        var s = Sec(dil, tr, en);
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
