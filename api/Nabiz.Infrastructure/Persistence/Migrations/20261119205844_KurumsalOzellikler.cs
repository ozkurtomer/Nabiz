using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nabiz.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KurumsalOzellikler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enps",
                schema: "nabiz",
                table: "Sorular",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Aksiyonlar",
                schema: "nabiz",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Oncelik = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SorumluKullaniciId = table.Column<int>(type: "int", nullable: true),
                    SonTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UyariId = table.Column<int>(type: "int", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: true),
                    OlusturanKullaniciId = table.Column<int>(type: "int", nullable: true),
                    Olusturma = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kapanma = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Not = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aksiyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aksiyonlar_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalSchema: "nabiz",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aksiyonlar_Kullanicilar_SorumluKullaniciId",
                        column: x => x.SorumluKullaniciId,
                        principalSchema: "nabiz",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Aksiyonlar_Uyarilar_UyariId",
                        column: x => x.UyariId,
                        principalSchema: "nabiz",
                        principalTable: "Uyarilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "Paketler",
                columns: new[] { "Id", "Aciklama", "AciklamaEn", "Ad", "AdEn", "Aktif" },
                values: new object[] { 10, "Çalışanların şirketi tavsiye etme olasılığını ölçen tek soru.", "A single question that measures how likely employees are to recommend the company.", "eNPS (Çalışan Tavsiye Skoru)", "eNPS (Employee Net Promoter Score)", true });

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 1,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 2,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 3,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 4,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 5,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 6,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 7,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 8,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 9,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 10,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 11,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 12,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 13,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 14,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 15,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 16,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 17,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 18,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 19,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 20,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 21,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 22,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 23,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 24,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 25,
                column: "Enps",
                value: false);

            migrationBuilder.UpdateData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 26,
                column: "Enps",
                value: false);

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "Sorular",
                columns: new[] { "Id", "Aktif", "Enps", "MetinEn", "MetinTr", "TakipMetniEn", "TakipMetniTr", "Tip" },
                values: new object[] { 27, true, true, "How likely are you to recommend this company to a friend as a place to work?", "Bu şirketi bir arkadaşına iş yeri olarak tavsiye etme olasılığın nedir?", "What influenced your score the most?", "Puanını en çok ne etkiledi?", "Puan" });

            migrationBuilder.InsertData(
                schema: "nabiz",
                table: "PaketSoru",
                columns: new[] { "PaketId", "SoruId", "Sira" },
                values: new object[] { 10, 27, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_Aksiyonlar_Durum",
                schema: "nabiz",
                table: "Aksiyonlar",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Aksiyonlar_KullaniciId",
                schema: "nabiz",
                table: "Aksiyonlar",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Aksiyonlar_SorumluKullaniciId",
                schema: "nabiz",
                table: "Aksiyonlar",
                column: "SorumluKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Aksiyonlar_UyariId",
                schema: "nabiz",
                table: "Aksiyonlar",
                column: "UyariId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aksiyonlar",
                schema: "nabiz");

            migrationBuilder.DeleteData(
                schema: "nabiz",
                table: "PaketSoru",
                keyColumns: new[] { "PaketId", "SoruId" },
                keyValues: new object[] { 10, 27 });

            migrationBuilder.DeleteData(
                schema: "nabiz",
                table: "Paketler",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "nabiz",
                table: "Sorular",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DropColumn(
                name: "Enps",
                schema: "nabiz",
                table: "Sorular");
        }
    }
}
