using System.Net;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

/// <summary>Tüm e-postalar alıcının diline göre (tr/en) hazırlanır; soru/paket metni o dilde yoksa diğer dile düşer.</summary>
public static class EmailSablonu
{
    /// <summary>Şirket markası (ad + vurgu rengi). MarkaService günceller; e-posta şablonları okur.</summary>
    public static (string Ad, string Renk) Marka = ("Nabız", "#1f4fc6");

    private static string Ad => WebUtility.HtmlEncode(Marka.Ad);
    private static string Renk => System.Text.RegularExpressions.Regex.IsMatch(Marka.Renk, "^#[0-9a-fA-F]{6}$") ? Marka.Renk : "#1f4fc6";
    private static string Ust => $"""<tr><td style="background:{Renk};padding:18px 28px;color:#ffffff;font-size:18px;font-weight:700;letter-spacing:.2px">{Ad}</td></tr>""";

    private static string Cerceve(int genislik, string icerik) => $$"""
<!doctype html>
<html><body style="margin:0;padding:24px;background:#f4f6f9;font-family:-apple-system,Segoe UI,Arial,sans-serif;color:#141a2a">
  <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td align="center">
    <table role="presentation" width="{{genislik}}" cellpadding="0" cellspacing="0" style="max-width:{{genislik}}px;background:#ffffff;border-radius:10px;overflow:hidden;border:1px solid #dfe3eb">
      {{Ust}}
      <tr><td style="padding:32px 28px;text-align:center">{{icerik}}</td></tr>
    </table>
  </td></tr></table>
</body></html>
""";

    private static string Buton(string link, string metin) =>
        $"""<a href="{link}" style="display:inline-block;margin-top:22px;padding:14px 32px;background:{Renk};color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;font-size:15px">{metin}</a>""";

    private static string Ilk(string adSoyad) => WebUtility.HtmlEncode(adSoyad.Split(' ')[0]);
    private static bool En(Kullanici k) => Nabiz.Domain.Dil.Normalize(k.Dil) == "en";

    /// <summary>Çalışana gönderilen soru paketi e-postası.</summary>
    public static (string Konu, string Html) Paket(Kullanici k, SoruPaketi paket, string link)
    {
        var en = En(k);
        var sorular = paket.Sorular.OrderBy(x => x.Sira).Select(x => x.Soru!).ToList();
        var ad = Ilk(k.Ad);
        var selam = en ? $"Hi {ad}" : $"Merhaba {ad}";

        string baslik, alt;
        if (sorular.Count == 1)
        {
            baslik = WebUtility.HtmlEncode(sorular[0].Metin(k.Dil));
            alt = en ? "It only takes a few seconds." : "Sadece birkaç saniyeni alır.";
        }
        else
        {
            baslik = WebUtility.HtmlEncode(paket.AdGoster(k.Dil));
            var dk = Math.Max(1, (int)Math.Ceiling(sorular.Count * 0.3));
            alt = en ? $"{sorular.Count} quick questions · about {dk} min" : $"{sorular.Count} kısa soru · yaklaşık {dk} dk";
        }
        // İlk soru emoji ise tek tıkla cevap: link, cevabı önceden seçili olarak açar.
        var ipucu = "";
        if (sorular.Count > 0 && sorular[0].Tip == SoruTipi.Emoji)
        {
            string[] yuzler = ["😞", "🙁", "😐", "🙂", "😄"];
            var sep = link.Contains('?') ? "&" : "?";
            var hucre = string.Concat(yuzler.Select((y, i) =>
                $"""<a href="{link}{sep}d={i + 1}" style="display:inline-block;margin:0 4px;padding:8px 10px;border:1px solid #e4e7ec;border-radius:10px;font-size:28px;text-decoration:none">{y}</a>"""));
            ipucu = $"<div style=\"margin:18px 0 0\">{hucre}</div>";
        }
        var konu = en ? $"{Marka.Ad}: a quick check-in for you" : $"{Marka.Ad}: sana kısa bir sorumuz var";

        return (konu, Cerceve(520, $$"""
        <p style="margin:0 0 8px;color:#667085">{{selam}}</p>
        <h1 style="margin:0;font-size:24px;line-height:1.3">{{baslik}}</h1>
        <p style="margin:10px 0 0;color:#667085">{{alt}}</p>
        {{ipucu}}
        {{Buton(link, en ? "Answer now" : "Hemen cevapla")}}
        <p style="margin:22px 0 0;color:#98a2b3;font-size:13px">{{(en ? "This link is personal, please don't share it." : "Bu link sana özeldir, lütfen paylaşma.")}}</p>
"""));
    }

    /// <summary>Şifresiz panel girişi için tek kullanımlık giriş linki e-postası.</summary>
    public static (string Konu, string Html) GirisLinki(Kullanici k, string link)
    {
        var en = En(k);
        var ad = Ilk(k.Ad);
        return (en ? $"{Marka.Ad}: your sign-in link" : $"{Marka.Ad}: giriş linkin", Cerceve(480, $$"""
        <p style="margin:0 0 8px;color:#667085">{{(en ? $"Hi {ad}" : $"Merhaba {ad}")}}</p>
        <p style="margin:0;font-size:17px">{{(en ? "Click the button to sign in. The link is valid for 15 minutes." : "Giriş yapmak için butona tıkla. Link 15 dakika geçerlidir.")}}</p>
        {{Buton(link, en ? "Sign in" : "Giriş yap")}}
        <p style="margin:22px 0 0;color:#98a2b3;font-size:13px">{{(en ? "If you didn't request this, you can ignore this email." : "Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin.")}}</p>
"""));
    }

    /// <summary>Cevaplamayanlara nazik hatırlatma.</summary>
    public static (string Konu, string Html) Hatirlatma(Kullanici k, SoruPaketi paket, string link)
    {
        var en = En(k);
        var ad = Ilk(k.Ad);
        var baslik = WebUtility.HtmlEncode(paket.AdGoster(k.Dil));
        return (en ? $"{Marka.Ad}: a gentle reminder" : $"{Marka.Ad}: küçük bir hatırlatma", Cerceve(520, $$"""
        <p style="margin:0 0 8px;color:#667085">{{(en ? $"Hi {ad}" : $"Merhaba {ad}")}}</p>
        <h1 style="margin:0;font-size:22px;line-height:1.3">{{(en ? "Your answer matters to us" : "Cevabın bizim için önemli")}}</h1>
        <p style="margin:10px 0 0;color:#667085">{{baslik}} · {{(en ? "it only takes a minute" : "sadece bir dakikanı alır")}}</p>
        {{Buton(link, en ? "Answer now" : "Hemen cevapla")}}
"""));
    }

    /// <summary>İK'ya düşük puan uyarısı (kişi bilgisi içerir; yalnızca İK'ya gider).</summary>
    public static (string Konu, string Html) IkUyari(Kullanici ik, Kullanici kisi, IReadOnlyList<(Soru Soru, Cevap Cevap)> dusukler, string link)
    {
        var en = En(ik);
        var anonimNot = en ? "(anonymous comment left)" : "(anonim yorum bırakıldı)";
        var satirlar = string.Concat(dusukler.Select(x =>
        {
            var yorum = string.IsNullOrWhiteSpace(x.Cevap.Yorum) ? "" : x.Cevap.YorumAnonim ? $"<br><i style=\"color:#667085\">{anonimNot}</i>" : $"<br><i style=\"color:#667085\">“{WebUtility.HtmlEncode(x.Cevap.Yorum)}”</i>";
            return $"<tr><td style=\"padding:8px 0;border-bottom:1px solid #eef0f6;text-align:left\">{WebUtility.HtmlEncode(x.Soru.Metin(ik.Dil))}{yorum}</td><td style=\"padding:8px 0 8px 12px;border-bottom:1px solid #eef0f6;color:#c0362c;font-weight:700;white-space:nowrap\">{Gosterim(x.Soru, x.Cevap.Deger, en)}</td></tr>";
        }));
        var konu = en ? $"{Marka.Ad}: low score alert - {kisi.Ad}" : $"{Marka.Ad}: düşük puan uyarısı - {kisi.Ad}";
        return (konu, Cerceve(560, $$"""
        <p style="margin:0 0 8px;color:#667085">{{(en ? "Hi" : "Merhaba")}} {{Ilk(ik.Ad)}},</p>
        <h1 style="margin:0 0 14px;font-size:20px">{{WebUtility.HtmlEncode(kisi.Ad)}} {{(en ? "gave a low score" : "düşük puan verdi")}}</h1>
        <p style="margin:0 0 12px;color:#667085">{{WebUtility.HtmlEncode(kisi.Departman ?? "—")}}</p>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{{satirlar}}</table>
        {{Buton(link, en ? "Open alerts" : "Uyarıları aç")}}
"""));
    }

    /// <summary>SMTP ayarlarının çalıştığını doğrulamak için gönderilen test e-postası.</summary>
    public static (string Konu, string Html) Test(string dil, string sunucu)
    {
        var en = Nabiz.Domain.Dil.Normalize(dil) == "en";
        return (en ? $"{Marka.Ad}: test email" : $"{Marka.Ad}: test e-postası", Cerceve(480, $$"""
        <h1 style="margin:8px 0 0;font-size:22px">{{(en ? "Email delivery works!" : "E-posta gönderimi çalışıyor!")}}</h1>
        <p style="margin:12px 0 0;color:#667085">{{(en ? "This test message was sent from Nabız using the SMTP server below." : "Bu test mesajı, Nabız tarafından aşağıdaki SMTP sunucusu üzerinden gönderildi.")}}</p>
        <p style="margin:14px 0 0;font-family:monospace;background:#f4f6f9;border-radius:8px;padding:8px 12px;display:inline-block">{{WebUtility.HtmlEncode(sunucu)}}</p>
"""));
    }

    /// <summary>Haftalık İK özeti: son 7 günün sayıları tek bakışta.</summary>
    public static (string Konu, string Html) HaftalikOzet(Kullanici ik, HaftalikOzetVerisi v, string link)
    {
        var en = En(ik);
        static string Kutu(string etiket, string deger, string renk = "#141a2a") =>
            $"<td style=\"padding:14px 10px;border:1px solid #e4e7ef;border-radius:8px;text-align:center\"><div style=\"font-size:12px;color:#667085\">{etiket}</div><div style=\"font-size:24px;font-weight:700;color:{renk}\">{deger}</div></td>";
        var skor = v.OrtalamaSkor is null ? "—" : v.OrtalamaSkor.Value.ToString("0.#");
        var fark = v.OncekiSkor is null || v.OrtalamaSkor is null ? "" : $" ({(v.OrtalamaSkor - v.OncekiSkor >= 0 ? "+" : "")}{(v.OrtalamaSkor - v.OncekiSkor):0.#})";
        var kutular = string.Concat(
            Kutu(en ? "Average score" : "Ortalama skor", skor + fark),
            Kutu(en ? "Response rate" : "Cevap oranı", $"%{v.CevapOrani:0}"),
            Kutu(en ? "New alerts" : "Yeni uyarı", v.YeniUyari.ToString(), v.YeniUyari > 0 ? "#c0362c" : "#141a2a"),
            Kutu(en ? "Overdue actions" : "Geciken aksiyon", v.GecikenAksiyon.ToString(), v.GecikenAksiyon > 0 ? "#c0362c" : "#141a2a"));
        return (en ? $"{Marka.Ad}: weekly summary" : $"{Marka.Ad}: haftalık özet", Cerceve(600, $$"""
        <p style="margin:0 0 8px;color:#667085;text-align:left">{{(en ? "Hi" : "Merhaba")}} {{Ilk(ik.Ad)}},</p>
        <h1 style="margin:0 0 18px;font-size:20px;text-align:left">{{(en ? "Your team's last 7 days" : "Son 7 günün özeti")}}</h1>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="6"><tr>{{kutular}}</tr></table>
        {{Buton(link, en ? "Open reports" : "Raporları aç")}}
"""));
    }

    private static string Gosterim(Soru s, int d, bool en) => s.Tip switch
    {
        SoruTipi.Emoji => new[] { "😞", "🙁", "😐", "🙂", "😄" }[Math.Clamp(d - 1, 0, 4)],
        SoruTipi.Puan => $"{d}/10",
        _ => d == 1 ? (en ? "Yes" : "Evet") : (en ? "No" : "Hayır"),
    };
}

/// <summary>Haftalık özet e-postasının sayıları.</summary>
public record HaftalikOzetVerisi(double? OrtalamaSkor, double? OncekiSkor, double CevapOrani, int YeniUyari, int GecikenAksiyon);
