using System.Text;

namespace Nabiz.Application.Services;

/// <summary>Türkçe Excel'in doğrudan açabildiği CSV: UTF-8 BOM + noktalı virgül ayracı.</summary>
public static class Csv
{
    public static byte[] Uret(IEnumerable<IEnumerable<object?>> satirlar)
    {
        var sb = new StringBuilder();
        foreach (var satir in satirlar)
            sb.AppendLine(string.Join(';', satir.Select(Hucre)));
        return new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Hucre(object? v)
    {
        var s = v switch { null => "", DateTime d => d.ToString("dd.MM.yyyy HH:mm"), bool b => b ? "Evet" : "Hayır", _ => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "" };
        // Excel formül enjeksiyonuna karşı: =,+,-,@ ile başlayan metinleri ' ile başlat
        if (s.Length > 0 && "=+-@".Contains(s[0]) && !double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _)) s = "'" + s;
        return s.Contains(';') || s.Contains('"') || s.Contains('\n') ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
    }
}
