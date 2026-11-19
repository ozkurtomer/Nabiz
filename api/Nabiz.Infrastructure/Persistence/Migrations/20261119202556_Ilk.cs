using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nabiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Ilk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nabiz");

            migrationBuilder.CreateTable(
                name: "Ayarlar",
                schema: "nabiz",
                columns: table => new
                {
                    Anahtar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Deger = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayarlar", x => x.Anahtar);
                });

            migrationBuilder.CreateTable(
                name: "Denetim",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zaman = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KullaniciId = table.Column<int>(type: "int", nullable: true),
                    KullaniciAd = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Eylem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detay = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denetim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Departman = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SifreHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dil = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Paketler",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AciklamaEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paketler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sorular",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetinTr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetinEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    TakipMetniTr = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TakipMetniEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sorular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GonderimPartileri",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaketId = table.Column<int>(type: "int", nullable: false),
                    HedefTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HedefDeger = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Zaman = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    Anlik = table.Column<bool>(type: "bit", nullable: false),
                    Demo = table.Column<bool>(type: "bit", nullable: false),
                    Test = table.Column<bool>(type: "bit", nullable: false),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GonderimPartileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GonderimPartileri_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalSchema: "nabiz",
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GonderimPlanlari",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaketId = table.Column<int>(type: "int", nullable: false),
                    HedefTipi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HedefDeger = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Baslangic = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SiklikGun = table.Column<int>(type: "int", nullable: false),
                    Saat = table.Column<TimeSpan>(type: "time", nullable: false),
                    SonCalisma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GonderimPlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GonderimPlanlari_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalSchema: "nabiz",
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaketSoru",
                schema: "nabiz",
                columns: table => new
                {
                    PaketId = table.Column<int>(type: "int", nullable: false),
                    SoruId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaketSoru", x => new { x.PaketId, x.SoruId });
                    table.ForeignKey(
                        name: "FK_PaketSoru_Paketler_PaketId",
                        column: x => x.PaketId,
                        principalSchema: "nabiz",
                        principalTable: "Paketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaketSoru_Sorular_SoruId",
                        column: x => x.SoruId,
                        principalSchema: "nabiz",
                        principalTable: "Sorular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gonderimler",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartiId = table.Column<int>(type: "int", nullable: false),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    GonderimZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Hata = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    TamamlanmaZamani = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HatirlatmaSayisi = table.Column<int>(type: "int", nullable: false),
                    SonHatirlatma = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gonderimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gonderimler_GonderimPartileri_PartiId",
                        column: x => x.PartiId,
                        principalSchema: "nabiz",
                        principalTable: "GonderimPartileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gonderimler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalSchema: "nabiz",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cevaplar",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GonderimId = table.Column<int>(type: "int", nullable: false),
                    SoruId = table.Column<int>(type: "int", nullable: false),
                    Deger = table.Column<int>(type: "int", nullable: false),
                    Yorum = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    YorumAnonim = table.Column<bool>(type: "bit", nullable: false),
                    GuncellemeZamani = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cevaplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cevaplar_Gonderimler_GonderimId",
                        column: x => x.GonderimId,
                        principalSchema: "nabiz",
                        principalTable: "Gonderimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cevaplar_Sorular_SoruId",
                        column: x => x.SoruId,
                        principalSchema: "nabiz",
                        principalTable: "Sorular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Uyarilar",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CevapId = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IkNotu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Olusturma = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uyarilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uyarilar_Cevaplar_CevapId",
                        column: x => x.CevapId,
                        principalSchema: "nabiz",
                        principalTable: "Cevaplar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "Paketler",
                columns: new[] { "Id", "Aciklama", "AciklamaEn", "Ad", "AdEn", "Aktif" },
                values: new object[,]
                {
                    { 1, "Tek soruluk hızlı moral kontrolü.", "A one-question quick mood check.", "Günlük Nabız", "Daily Pulse", true },
                    { 2, "Değer görme, motivasyon, bağlılık ve enerji.", "Feeling valued, motivation, commitment and energy.", "Motivasyon Paketi", "Motivation Pack", true },
                    { 3, "İş yükü, dinlenme ve iş-yaşam dengesi.", "Workload, rest and work-life balance.", "İş Yükü ve Denge", "Workload & Balance", true },
                    { 4, "Ekip iletişimi ve yönetici desteği.", "Team communication and manager support.", "Ekip ve Yönetici", "Team & Manager", true },
                    { 5, "İşe yeni başlayanların ilk haftalardaki deneyimi.", "How new joiners experience their first weeks.", "Oryantasyon (Yeni Başlayanlar)", "Onboarding (New Joiners)", true },
                    { 6, "Uzaktan çalışmada bağlantı, verimlilik ve ortam.", "Connection, productivity and setup when working remotely.", "Uzaktan / Hibrit Çalışma", "Remote / Hybrid Work", true },
                    { 7, "Toplantıdan hemen sonra gönderilen hızlı değerlendirme.", "A quick evaluation sent right after a meeting.", "Toplantı Sonrası Geri Bildirim", "Post-Meeting Feedback", true },
                    { 8, "Kariyer beklentisi, ücret memnuniyeti ve bağlılık; ayrılma riskini erken görmek için.", "Career outlook, pay satisfaction and commitment; to spot attrition risk early.", "Bağlılık ve Kariyer", "Commitment & Career", true },
                    { 9, "Yöneticiye güven, geri bildirim ve adalet algısı.", "Trust, feedback and fairness perceptions about the manager.", "Yönetici Değerlendirme", "Manager Feedback", true }
                });

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "Sorular",
                columns: new[] { "Id", "Aktif", "MetinEn", "MetinTr", "TakipMetniEn", "TakipMetniTr", "Tip" },
                values: new object[,]
                {
                    { 1, true, "Are you feeling good today?", "Bugün keyfin yerinde mi?", "What made today harder?", "Bugünü zorlaştıran ne oldu?", "Emoji" },
                    { 2, true, "How energetic did you feel this week?", "Bu hafta kendini ne kadar enerjik hissettin?", null, null, "Puan" },
                    { 3, true, "Do you feel valued at work?", "İşyerinde kendini değerli hissediyor musun?", "What would make you feel more valued?", "Kendini daha değerli hissetmen için ne değişmeli?", "Emoji" },
                    { 4, true, "Is your workload manageable right now?", "İş yükün şu an yönetilebilir düzeyde mi?", "What makes your workload hard?", "İş yükünü en çok ne zorlaştırıyor?", "EvetHayir" },
                    { 5, true, "How is your communication with teammates?", "Ekip arkadaşlarınla iletişimin nasıl?", null, null, "Puan" },
                    { 6, true, "Do you get enough support from your manager?", "Yöneticinden yeterli destek alıyor musun?", "What kind of support do you need from your manager?", "Yöneticinden nasıl bir desteğe ihtiyacın var?", "EvetHayir" },
                    { 7, true, "Were you motivated to come to work this week?", "Bu hafta işe gelmek için motive miydin?", "What lowered your motivation the most?", "Motivasyonunu en çok ne düşürdü?", "Emoji" },
                    { 8, true, "Are you getting enough sleep and rest?", "Uykun ve dinlenmen yeterli mi?", null, null, "Emoji" },
                    { 9, true, "How connected do you feel to the company?", "Şirkete ne kadar bağlı hissediyorsun?", null, null, "Puan" },
                    { 10, true, "How was your work-life balance this week?", "Bu hafta iş ve özel hayat dengen nasıldı?", null, null, "Puan" },
                    { 11, true, "Have you received enough onboarding since you started?", "İşe başladığından beri yeterli oryantasyon aldın mı?", "What was missing?", "Neler eksik kaldı?", "EvetHayir" },
                    { 12, true, "Do you clearly know your role and responsibilities?", "Görev ve sorumluluklarını net olarak biliyor musun?", null, null, "Puan" },
                    { 13, true, "Do you feel you fit in with the team?", "Ekibe uyum sağladığını hissediyor musun?", null, null, "Emoji" },
                    { 14, true, "Do you have the tools and equipment you need?", "İşini yapmak için gereken araç ve ekipmana sahip misin?", "What do you need?", "Neye ihtiyacın var?", "EvetHayir" },
                    { 15, true, "Can you stay connected with your team while working remotely?", "Uzaktan/hibrit çalışırken ekibinle bağlantıda kalabiliyor musun?", null, null, "Emoji" },
                    { 16, true, "Are you productive when working from home?", "Evden çalışırken verimli olabiliyor musun?", null, null, "Puan" },
                    { 17, true, "Is your work setup (equipment, internet) sufficient?", "Çalışma ortamın (ekipman, internet) yeterli mi?", "What should we improve?", "Neyi iyileştirmemizi istersin?", "EvetHayir" },
                    { 18, true, "Did the meeting achieve its goal?", "Toplantı amacına ulaştı mı?", "Why not?", "Neden ulaşamadı?", "EvetHayir" },
                    { 19, true, "Could you freely express your views in the meeting?", "Toplantıda görüşlerini rahatça ifade edebildin mi?", null, null, "Emoji" },
                    { 20, true, "Was the meeting time used efficiently?", "Toplantı süresi verimli kullanıldı mı?", null, null, "Puan" },
                    { 21, true, "Do you see a long-term career here?", "Bu şirkette uzun vadeli bir kariyer görüyor musun?", "What makes you doubt it?", "Seni en çok ne düşündürüyor?", "EvetHayir" },
                    { 22, true, "Are you satisfied with career development opportunities?", "Kariyer gelişim fırsatlarından memnun musun?", null, null, "Puan" },
                    { 23, true, "Are you satisfied with your pay and benefits?", "Ücret ve yan haklarından memnun musun?", null, null, "Puan" },
                    { 24, true, "Does your manager give you regular, clear feedback?", "Yöneticin sana düzenli ve net geri bildirim veriyor mu?", null, null, "EvetHayir" },
                    { 25, true, "Do you trust your manager?", "Yöneticine güveniyor musun?", "What would increase your trust?", "Güveninin artması için ne gerekir?", "Emoji" },
                    { 26, true, "Is your manager fair in decisions?", "Yöneticin kararlarında adil mi?", null, null, "Puan" }
                });

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "PaketSoru",
                columns: new[] { "PaketId", "SoruId", "Sira" },
                values: new object[,]
                {
                    { 1, 1, 0 },
                    { 2, 2, 3 },
                    { 2, 3, 0 },
                    { 2, 7, 1 },
                    { 2, 9, 2 },
                    { 3, 4, 0 },
                    { 3, 8, 1 },
                    { 3, 10, 2 },
                    { 4, 5, 0 },
                    { 4, 6, 1 },
                    { 5, 11, 0 },
                    { 5, 12, 1 },
                    { 5, 13, 2 },
                    { 5, 14, 3 },
                    { 6, 10, 3 },
                    { 6, 15, 0 },
                    { 6, 16, 1 },
                    { 6, 17, 2 },
                    { 7, 18, 0 },
                    { 7, 19, 1 },
                    { 7, 20, 2 },
                    { 8, 9, 3 },
                    { 8, 21, 0 },
                    { 8, 22, 1 },
                    { 8, 23, 2 },
                    { 9, 6, 0 },
                    { 9, 24, 1 },
                    { 9, 25, 2 },
                    { 9, 26, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cevaplar_GonderimId_SoruId",
                schema: "nabiz",
                table: "Cevaplar",
                columns: new[] { "GonderimId", "SoruId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cevaplar_SoruId",
                schema: "nabiz",
                table: "Cevaplar",
                column: "SoruId");

            migrationBuilder.CreateIndex(
                name: "IX_Denetim_Zaman",
                schema: "nabiz",
                table: "Denetim",
                column: "Zaman");

            migrationBuilder.CreateIndex(
                name: "IX_Gonderimler_GonderimZamani",
                schema: "nabiz",
                table: "Gonderimler",
                column: "GonderimZamani");

            migrationBuilder.CreateIndex(
                name: "IX_Gonderimler_KullaniciId",
                schema: "nabiz",
                table: "Gonderimler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Gonderimler_PartiId",
                schema: "nabiz",
                table: "Gonderimler",
                column: "PartiId");

            migrationBuilder.CreateIndex(
                name: "IX_Gonderimler_Token",
                schema: "nabiz",
                table: "Gonderimler",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GonderimPartileri_PaketId",
                schema: "nabiz",
                table: "GonderimPartileri",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_GonderimPartileri_Zaman",
                schema: "nabiz",
                table: "GonderimPartileri",
                column: "Zaman");

            migrationBuilder.CreateIndex(
                name: "IX_GonderimPlanlari_PaketId",
                schema: "nabiz",
                table: "GonderimPlanlari",
                column: "PaketId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_Email",
                schema: "nabiz",
                table: "Kullanicilar",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_PersonelId",
                schema: "nabiz",
                table: "Kullanicilar",
                column: "PersonelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaketSoru_SoruId",
                schema: "nabiz",
                table: "PaketSoru",
                column: "SoruId");

            migrationBuilder.CreateIndex(
                name: "IX_Uyarilar_CevapId",
                schema: "nabiz",
                table: "Uyarilar",
                column: "CevapId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ayarlar",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Denetim",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "GonderimPlanlari",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "PaketSoru",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Uyarilar",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Cevaplar",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Gonderimler",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Sorular",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "GonderimPartileri",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Kullanicilar",
                schema: "nabiz");

            migrationBuilder.DropTable(
                name: "Paketler",
                schema: "nabiz");
        }
    }
}
