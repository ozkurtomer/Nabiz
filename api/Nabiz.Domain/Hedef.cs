namespace Nabiz.Domain;

public static class Hedef
{
    /// <summary>"3,7,12" biçimindeki kişi listesini Id'lere çevirir.</summary>
    public static List<int> KisiIdleri(string? deger) =>
        (deger ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var n) ? n : 0).Where(n => n > 0).Distinct().ToList();
}
