using System.Net;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public static class EmailSablonu
{
    private const string Ust = """<tr><td style="background:linear-gradient(135deg,#4f5df0,#7a4df2);padding:22px 28px;color:#fff;font-size:20px;font-weight:700">♡ Nabız</td></tr>""";

    private static string Cerceve(int genislik, string icerik) => $$"""
<!doctype html>
<html><body style="margin:0;padding:24px;background:#f4f5fb;font-family:-apple-system,Segoe UI,Arial,sans-serif;color:#141a2a">
  <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td align="center">
    <table role="presentation" width="{{genislik}}" cellpadding="0" cellspacing="0" style="max-width:{{genislik}}px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e4e7ef">
      {{Ust}}
      <tr><td style="padding:32px 28px;text-align:center">{{icerik}}</td></tr>
    </table>
  </td></tr></table>
</body></html>
""";

    private static string Buton(string link, string metin) =>
        $"""<a href="{link}" style="display:inline-block;margin-top:22px;padding:14px 32px;background:#4f5df0;color:#ffffff;text-decoration:none;border-radius:12px;font-weight:700;font-size:16px">{metin} →</a>""";

    /// <summary>Çalışana gönderilen soru paketi e-postası (kullanıcının diline göre).</summary>
    public static (string Konu, string Html) Paket(Kullanici k, SoruPaketi paket, string link)
    {
        var en = k.Dil == "en";
        var sorular = paket.Sorular.OrderBy(x => x.Sira).Select(x => x.Soru!).ToList();
        var ad = WebUtility.HtmlEncode(k.Ad.Split(' ')[0]);
        var selam = en ? $"Hi {ad} 👋" : $"Merhaba {ad} 👋";

        string baslik, alt;
        if (sorular.Count == 1)
        {
            baslik = WebUtility.HtmlEncode(en ? sorular[0].MetinEn : sorular[0].MetinTr);
            alt = en ? "It only takes a few seconds." : "Sadece birkaç saniyeni alır.";
        }
        else
        {
            baslik = WebUtility.HtmlEncode(paket.Ad);
            var dk = Math.Max(1, (int)Math.Ceiling(sorular.Count * 0.3));
            alt = en ? $"{sorular.Count} quick questions · about {dk} min" : $"{sorular.Count} kısa soru · yaklaşık {dk} dk";
        }
        var ipucu = sorular.Count == 1 && sorular[0].Tip == SoruTipi.Emoji
            ? "<div style=\"font-size:30px;letter-spacing:8px;margin:18px 0 0\">😞 🙁 😐 🙂 😄</div>" : "";
        var konu = en ? "Nabız: a quick check-in for you" : "Nabız: sana kısa bir sorumuz var";

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
        var en = k.Dil == "en";
        var ad = WebUtility.HtmlEncode(k.Ad.Split(' ')[0]);
        var konu = en ? "Nabız: your sign-in link" : "Nabız: giriş linkin";
        return (konu, Cerceve(480, $$"""
        <p style="margin:0 0 8px;color:#667085">{{(en ? $"Hi {ad} 👋" : $"Merhaba {ad} 👋")}}</p>
        <p style="margin:0;font-size:17px">{{(en ? "Click the button to sign in. The link is valid for 15 minutes." : "Giriş yapmak için butona tıkla. Link 15 dakika geçerlidir.")}}</p>
        {{Buton(link, en ? "Sign in" : "Giriş yap")}}
        <p style="margin:22px 0 0;color:#98a2b3;font-size:13px">{{(en ? "If you didn't request this, you can ignore this email." : "Bu isteği sen yapmadıysan bu e-postayı yok sayabilirsin.")}}</p>
"""));
    }

    /// <summary>Cevaplamayanlara nazik hatırlatma.</summary>
    public static (string Konu, string Html) Hatirlatma(Kullanici k, SoruPaketi paket, string link)
    {
        var en = k.Dil == "en";
        var ad = WebUtility.HtmlEncode(k.Ad.Split(' ')[0]);
        var baslik = WebUtility.HtmlEncode(paket.Ad);
        return (en ? "Nabız: a gentle reminder" : "Nabız: küçük bir hatırlatma", Cerceve(520, $$"""
        <p style="margin:0 0 8px;color:#667085">{{(en ? $"Hi {ad}" : $"Merhaba {ad}")}} 🙂</p>
        <h1 style="margin:0;font-size:22px;line-height:1.3">{{(en ? "Your answer matters to us" : "Cevabın bizim için önemli")}}</h1>
        <p style="margin:10px 0 0;color:#667085">{{baslik}} · {{(en ? "it only takes a minute" : "sadece bir dakikanı alır")}}</p>
        {{Buton(link, en ? "Answer now" : "Hemen cevapla")}}
"""));
    }

    /// <summary>İK'ya düşük puan uyarısı (kişi bilgisi içerir; yalnızca İK/Admin'e gider).</summary>
    public static (string Konu, string Html) IkUyari(Kullanici ik, Kullanici kisi, IReadOnlyList<(Soru Soru, Cevap Cevap)> dusukler, string link)
    {
        var satirlar = string.Concat(dusukler.Select(x =>
        {
            var yorum = string.IsNullOrWhiteSpace(x.Cevap.Yorum) ? "" : x.Cevap.YorumAnonim ? "<br><i style=\"color:#667085\">(anonim yorum bırakıldı)</i>" : $"<br><i style=\"color:#667085\">“{WebUtility.HtmlEncode(x.Cevap.Yorum)}”</i>";
            return $"<tr><td style=\"padding:8px 0;border-bottom:1px solid #eef0f6;text-align:left\">{WebUtility.HtmlEncode(x.Soru.MetinTr)}{yorum}</td><td style=\"padding:8px 0 8px 12px;border-bottom:1px solid #eef0f6;color:#d92d20;font-weight:700;white-space:nowrap\">{Gosterim(x.Soru, x.Cevap.Deger)}</td></tr>";
        }));
        return ($"Nabız: düşük puan uyarısı - {kisi.Ad}", Cerceve(560, $$"""
        <p style="margin:0 0 8px;color:#667085">Merhaba {{Kodla(ik.Ad.Split(' ')[0])}},</p>
        <h1 style="margin:0 0 14px;font-size:20px">⚠️ {{WebUtility.HtmlEncode(kisi.Ad)}} düşük puan verdi</h1>
        <p style="margin:0 0 12px;color:#667085">{{WebUtility.HtmlEncode(kisi.Departman ?? "—")}}</p>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{{satirlar}}</table>
        {{Buton(link, "Uyarıları aç")}}
"""));
    }

    private static string Kodla(string s) => WebUtility.HtmlEncode(s);

    private static string Gosterim(Soru s, int d) => s.Tip switch
    {
        SoruTipi.Emoji => new[] { "😞", "🙁", "😐", "🙂", "😄" }[Math.Clamp(d - 1, 0, 4)],
        SoruTipi.Puan => $"{d}/10",
        _ => d == 1 ? "Evet" : "Hayır",
    };
}
