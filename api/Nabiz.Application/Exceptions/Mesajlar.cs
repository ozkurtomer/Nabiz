namespace Nabiz.Application.Exceptions;

/// <summary>Kullanıcıya gösterilen hata mesajlarının İngilizce karşılıkları (kaynak metin Türkçe).</summary>
public static class Mesajlar
{
    private static readonly Dictionary<string, string> En = new()
    {
        ["E-posta veya şifre hatalı."] = "Incorrect email or password.",
        ["Çok fazla hatalı deneme. Lütfen 15 dakika sonra tekrar dene."] = "Too many failed attempts. Please try again in 15 minutes.",
        ["E-posta zorunlu."] = "Email is required.",
        ["Geçersiz mobil anahtar."] = "Invalid mobile key.",
        ["Giriş linki geçersiz ya da süresi dolmuş."] = "The sign-in link is invalid or has expired.",
        ["Hesap bulunamadı."] = "Account not found.",
        ["Kullanıcı bulunamadı."] = "User not found.",
        ["Emoji eşiği 1-5 arasında olmalı."] = "The emoji threshold must be between 1 and 5.",
        ["Puan eşiği 1-10 arasında olmalı."] = "The score threshold must be between 1 and 10.",
        ["Bu bağlantı geçersiz ya da süresi dolmuş."] = "This link is invalid or has expired.",
        ["Bu soru pakette yok."] = "This question is not in the package.",
        ["Cevaplar tamamlandı, değiştirilemez."] = "The answers are completed and can no longer be changed.",
        ["Geçersiz cevap değeri."] = "Invalid answer value.",
        ["Paket bulunamadı."] = "Package not found.",
        ["Tamamlamak için tüm soruları cevaplamalısın."] = "Please answer all questions to complete.",
        ["Hedef kitlede gönderilecek aktif kullanıcı yok."] = "There are no active users to send to in the selected audience.",
        ["Departman seçilmeli."] = "Please choose a department.",
        ["En az bir kişi seçilmeli."] = "Please choose at least one person.",
        ["Seçilen kişilerden bazıları bulunamadı."] = "Some of the selected people could not be found.",
        ["Bu paket planlarda veya geçmiş gönderimlerde kullanılıyor; silmek yerine pasif yapabilirsin."] = "This package is used in schedules or past sends; set it inactive instead of deleting it.",
        ["Paket adı zorunlu (Türkçe ya da İngilizce)."] = "A package name is required (Turkish or English).",
        ["Pakete en az bir soru eklemelisin."] = "Add at least one question to the package.",
        ["Seçilen sorulardan bazıları bulunamadı."] = "Some of the selected questions could not be found.",
        ["Saat biçimi SS:dd olmalı."] = "Time format must be HH:mm.",
        ["Sıklık geçersiz (0 = bir kez, 1+ = kaç günde bir)."] = "Invalid frequency (0 = once, 1+ = every N days).",
        ["Başlangıç tarihi bitişten sonra olamaz."] = "The start date cannot be after the end date.",
        ["En fazla 400 günlük aralık seçilebilir."] = "You can select a range of at most 400 days.",
        ["Raporları ve kişi bazlı verileri yalnızca İK görebilir."] = "Only HR can view reports and person-level data.",
        ["Soru metnini Türkçe ya da İngilizce olarak yazmalısın (en az biri zorunlu)."] = "Write the question text in Turkish or English (at least one is required).",
        ["Geçerli bir e-posta adresi gir."] = "Enter a valid email address.",
        ["Yetkisiz."] = "Unauthorized.",
        ["Kayıt bulunamadı."] = "Record not found.",
        ["Bu soru bulunamadı."] = "Question not found.",
        ["Soru bulunamadı."] = "Question not found.",
        ["Beklenmeyen bir hata oluştu."] = "An unexpected error occurred.",
    };

    public static string Cevir(string tr, string dil) =>
        dil == "en" && En.TryGetValue(tr, out var en) ? en : tr;
}
