using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Infrastructure.Persistence;

/// <summary>Başlangıç soru havuzu ve hazır paket şablonları. İK panelinden düzenlenebilir, yenileri eklenebilir.</summary>
public static class SoruSeed
{
    public static readonly Soru[] Sorular =
    [
        S(1, "Bugün keyfin yerinde mi?", "Are you feeling good today?", SoruTipi.Emoji, "Bugünü zorlaştıran ne oldu?", "What made today harder?"),
        S(2, "Bu hafta kendini ne kadar enerjik hissettin?", "How energetic did you feel this week?", SoruTipi.Puan),
        S(3, "İşyerinde kendini değerli hissediyor musun?", "Do you feel valued at work?", SoruTipi.Emoji, "Kendini daha değerli hissetmen için ne değişmeli?", "What would make you feel more valued?"),
        S(4, "İş yükün şu an yönetilebilir düzeyde mi?", "Is your workload manageable right now?", SoruTipi.EvetHayir, "İş yükünü en çok ne zorlaştırıyor?", "What makes your workload hard?"),
        S(5, "Ekip arkadaşlarınla iletişimin nasıl?", "How is your communication with teammates?", SoruTipi.Puan),
        S(6, "Yöneticinden yeterli destek alıyor musun?", "Do you get enough support from your manager?", SoruTipi.EvetHayir, "Yöneticinden nasıl bir desteğe ihtiyacın var?", "What kind of support do you need from your manager?"),
        S(7, "Bu hafta işe gelmek için motive miydin?", "Were you motivated to come to work this week?", SoruTipi.Emoji, "Motivasyonunu en çok ne düşürdü?", "What lowered your motivation the most?"),
        S(8, "Uykun ve dinlenmen yeterli mi?", "Are you getting enough sleep and rest?", SoruTipi.Emoji),
        S(9, "Şirkete ne kadar bağlı hissediyorsun?", "How connected do you feel to the company?", SoruTipi.Puan),
        S(10, "Bu hafta iş ve özel hayat dengen nasıldı?", "How was your work-life balance this week?", SoruTipi.Puan),
        // Oryantasyon
        S(11, "İşe başladığından beri yeterli oryantasyon aldın mı?", "Have you received enough onboarding since you started?", SoruTipi.EvetHayir, "Neler eksik kaldı?", "What was missing?"),
        S(12, "Görev ve sorumluluklarını net olarak biliyor musun?", "Do you clearly know your role and responsibilities?", SoruTipi.Puan),
        S(13, "Ekibe uyum sağladığını hissediyor musun?", "Do you feel you fit in with the team?", SoruTipi.Emoji),
        S(14, "İşini yapmak için gereken araç ve ekipmana sahip misin?", "Do you have the tools and equipment you need?", SoruTipi.EvetHayir, "Neye ihtiyacın var?", "What do you need?"),
        // Uzaktan / hibrit
        S(15, "Uzaktan/hibrit çalışırken ekibinle bağlantıda kalabiliyor musun?", "Can you stay connected with your team while working remotely?", SoruTipi.Emoji),
        S(16, "Evden çalışırken verimli olabiliyor musun?", "Are you productive when working from home?", SoruTipi.Puan),
        S(17, "Çalışma ortamın (ekipman, internet) yeterli mi?", "Is your work setup (equipment, internet) sufficient?", SoruTipi.EvetHayir, "Neyi iyileştirmemizi istersin?", "What should we improve?"),
        // Toplantı sonrası
        S(18, "Toplantı amacına ulaştı mı?", "Did the meeting achieve its goal?", SoruTipi.EvetHayir, "Neden ulaşamadı?", "Why not?"),
        S(19, "Toplantıda görüşlerini rahatça ifade edebildin mi?", "Could you freely express your views in the meeting?", SoruTipi.Emoji),
        S(20, "Toplantı süresi verimli kullanıldı mı?", "Was the meeting time used efficiently?", SoruTipi.Puan),
        // Bağlılık ve kariyer
        S(21, "Bu şirkette uzun vadeli bir kariyer görüyor musun?", "Do you see a long-term career here?", SoruTipi.EvetHayir, "Seni en çok ne düşündürüyor?", "What makes you doubt it?"),
        S(22, "Kariyer gelişim fırsatlarından memnun musun?", "Are you satisfied with career development opportunities?", SoruTipi.Puan),
        S(23, "Ücret ve yan haklarından memnun musun?", "Are you satisfied with your pay and benefits?", SoruTipi.Puan),
        // Yönetici değerlendirme
        S(24, "Yöneticin sana düzenli ve net geri bildirim veriyor mu?", "Does your manager give you regular, clear feedback?", SoruTipi.EvetHayir),
        S(25, "Yöneticine güveniyor musun?", "Do you trust your manager?", SoruTipi.Emoji, "Güveninin artması için ne gerekir?", "What would increase your trust?"),
        S(26, "Yöneticin kararlarında adil mi?", "Is your manager fair in decisions?", SoruTipi.Puan),
    ];

    public static readonly SoruPaketi[] Paketler =
    [
        new() { Id = 1, AdEn = "Daily Pulse", AciklamaEn = "A one-question quick mood check.", Ad = "Günlük Nabız", Aciklama = "Tek soruluk hızlı moral kontrolü.", Aktif = true },
        new() { Id = 2, AdEn = "Motivation Pack", AciklamaEn = "Feeling valued, motivation, commitment and energy.", Ad = "Motivasyon Paketi", Aciklama = "Değer görme, motivasyon, bağlılık ve enerji.", Aktif = true },
        new() { Id = 3, AdEn = "Workload & Balance", AciklamaEn = "Workload, rest and work-life balance.", Ad = "İş Yükü ve Denge", Aciklama = "İş yükü, dinlenme ve iş-yaşam dengesi.", Aktif = true },
        new() { Id = 4, AdEn = "Team & Manager", AciklamaEn = "Team communication and manager support.", Ad = "Ekip ve Yönetici", Aciklama = "Ekip iletişimi ve yönetici desteği.", Aktif = true },
        new() { Id = 5, AdEn = "Onboarding (New Joiners)", AciklamaEn = "How new joiners experience their first weeks.", Ad = "Oryantasyon (Yeni Başlayanlar)", Aciklama = "İşe yeni başlayanların ilk haftalardaki deneyimi.", Aktif = true },
        new() { Id = 6, AdEn = "Remote / Hybrid Work", AciklamaEn = "Connection, productivity and setup when working remotely.", Ad = "Uzaktan / Hibrit Çalışma", Aciklama = "Uzaktan çalışmada bağlantı, verimlilik ve ortam.", Aktif = true },
        new() { Id = 7, AdEn = "Post-Meeting Feedback", AciklamaEn = "A quick evaluation sent right after a meeting.", Ad = "Toplantı Sonrası Geri Bildirim", Aciklama = "Toplantıdan hemen sonra gönderilen hızlı değerlendirme.", Aktif = true },
        new() { Id = 8, AdEn = "Commitment & Career", AciklamaEn = "Career outlook, pay satisfaction and commitment; to spot attrition risk early.", Ad = "Bağlılık ve Kariyer", Aciklama = "Kariyer beklentisi, ücret memnuniyeti ve bağlılık; ayrılma riskini erken görmek için.", Aktif = true },
        new() { Id = 9, AdEn = "Manager Feedback", AciklamaEn = "Trust, feedback and fairness perceptions about the manager.", Ad = "Yönetici Değerlendirme", Aciklama = "Yöneticiye güven, geri bildirim ve adalet algısı.", Aktif = true },
    ];

    public static readonly PaketSoru[] PaketSorulari =
    [
        P(1, 1, 0),
        P(2, 3, 0), P(2, 7, 1), P(2, 9, 2), P(2, 2, 3),
        P(3, 4, 0), P(3, 8, 1), P(3, 10, 2),
        P(4, 5, 0), P(4, 6, 1),
        P(5, 11, 0), P(5, 12, 1), P(5, 13, 2), P(5, 14, 3),
        P(6, 15, 0), P(6, 16, 1), P(6, 17, 2), P(6, 10, 3),
        P(7, 18, 0), P(7, 19, 1), P(7, 20, 2),
        P(8, 21, 0), P(8, 22, 1), P(8, 23, 2), P(8, 9, 3),
        P(9, 6, 0), P(9, 24, 1), P(9, 25, 2), P(9, 26, 3),
    ];

    private static Soru S(int id, string tr, string en, SoruTipi tip, string? takipTr = null, string? takipEn = null) =>
        new() { Id = id, MetinTr = tr, MetinEn = en, Tip = tip, Aktif = true, TakipMetniTr = takipTr, TakipMetniEn = takipEn };
    private static PaketSoru P(int paket, int soru, int sira) => new() { PaketId = paket, SoruId = soru, Sira = sira };
}
