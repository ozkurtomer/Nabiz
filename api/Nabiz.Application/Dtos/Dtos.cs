using Nabiz.Domain.Enums;

namespace Nabiz.Application.Dtos;

// ---- Auth
public record LoginIstek(string Email, string Sifre);
public record LoginYanit(string Token, string Rol, string Dil, string Ad);
public record GirisLinkiIstek(string Email);
public record GirisDogrulaIstek(string Token);

// ---- Soru ve paket
public record SoruDto(int Id, string MetinTr, string MetinEn, SoruTipi Tip, bool Aktif, string? TakipMetniTr = null, string? TakipMetniEn = null, bool Enps = false);
public record SoruKaydetIstek(string? MetinTr, string? MetinEn, SoruTipi Tip, bool Aktif, string? TakipMetniTr = null, string? TakipMetniEn = null, bool Enps = false);
public record PaketDto(int Id, string Ad, string AdEn, string? Aciklama, string? AciklamaEn, bool Aktif, List<SoruDto> Sorular);
public record PaketKaydetIstek(string? Ad, string? AdEn, string? Aciklama, string? AciklamaEn, bool Aktif, List<int> SoruIdleri);

// ---- Kullanıcı
public record KullaniciDto(int Id, string PersonelId, string Ad, string Email, string? Departman, Rol Rol, string Dil, bool Aktif);
public record KullaniciGuncelleIstek(Rol Rol, bool Aktif, string Dil);
public record PersonelAktarimSonucu(int Yeni, int Guncel, int Atlanan);

// ---- Plan
public record PlanDto(int Id, int PaketId, string? PaketAd, HedefTipi HedefTipi, string? HedefDeger, string HedefAciklama, DateTime Baslangic, int SiklikGun, string Saat, bool Aktif, DateTime? SonCalisma = null);
public record PlanKaydetIstek(int PaketId, HedefTipi HedefTipi, string? HedefDeger, DateTime Baslangic, int SiklikGun, string Saat, bool Aktif);

// ---- Cevaplama (çalışan)
public record CevapGirdisi(int SoruId, int Deger, string? Yorum, bool AnonimYorum = false);
public record CevapKaydetIstek(List<CevapGirdisi> Cevaplar, bool Tamamla);
public record CevapSayfasiDto(string KullaniciAd, string Dil, string PaketAd, string PaketAdEn, string? Aciklama, string? AciklamaEn, List<SoruDto> Sorular, List<CevapGirdisi> Cevaplar, bool Tamamlandi, int EmojiEsik, int PuanEsik, string Aydinlatma = "", string SirketAd = "Nabız", string Renk = "#1f4fc6");
public record CevapKaydetYanit(bool Tamamlandi, int Cevaplanan, int Toplam);
public record TestGonderimYanit(string Token);
public record BenimSoruDto(string Token, string Paket, int SoruSayisi, DateTime Zaman, bool Tamamlandi);

// ---- Uyarı
public record UyariDto(int Id, DateTime Olusturma, UyariDurumu Durum, string? IkNotu, int KullaniciId, string KullaniciAd, string? Departman, string Soru, int Deger, string? Yorum, bool YorumAnonim, int SonDonemUyariSayisi);
public record UyariGuncelleIstek(UyariDurumu Durum, string? IkNotu);

// ---- Ayar
public record AyarlarDto(int EmojiEsik, int PuanEsik);

// ---- Gönderim (anlık gönderim + gönderim raporu)
public record GonderimSonucu(int Toplam, int Basarili, int Hatali, string? IlkHata);
public record AnlikGonderIstek(int PaketId, HedefTipi HedefTipi, string? HedefDeger);
public record AnlikGonderYanit(int PartiId, GonderimSonucu Sonuc);
public record PartiDto(int Id, DateTime Zaman, int PaketId, string Paket, int SoruSayisi, string Hedef, bool Anlik, bool Demo, bool Test, int Gonderilen, int Hatali, int Cevaplayan, double CevapOrani, double? OrtalamaSkor);
public record DagilimNoktasi(int Deger, int Adet);
public record SoruDagilimi(int SoruId, string Soru, SoruTipi Tip, int CevapSayisi, double? OrtalamaSkor, List<DagilimNoktasi> Dagilim);
public record KisiCevabi(int SoruId, string Soru, SoruTipi Tip, int Deger, double Skor, string? Yorum, bool YorumAnonim, bool Dusuk);
public record PartiKisiSatiri(int KullaniciId, string Ad, string Email, string? Departman, string GonderimDurumu, string? Hata, bool Tamamlandi, double? OrtalamaSkor, bool Dusuk, DateTime? TamamlanmaZamani, List<KisiCevabi> Cevaplar);
public record AnonimYorumSatiri(string Soru, string Yorum);
public record PartiDetayDto(PartiDto Parti, List<SoruDagilimi> Sorular, List<PartiKisiSatiri> Kisiler, List<AnonimYorumSatiri> AnonimYorumlar);

// ---- Raporlar
public record RaporOzet(int Gonderilen, int Cevaplayan, double CevapOrani, double? OrtalamaSkor, int Olumlu, int Notr, int Olumsuz, int DusukPuan);
public record TrendNoktasi(DateTime Gun, double? OrtalamaSkor, int CevapSayisi);
public record DepartmanSatiri(string Departman, int Gonderilen, int Cevaplayan, double CevapOrani, double? OrtalamaSkor, int Olumlu, int Notr, int Olumsuz, double? OncekiOrtalamaSkor = null);
public record SoruSatiri(int SoruId, string Soru, SoruTipi Tip, int CevapSayisi, double? OrtalamaSkor, List<DagilimNoktasi> Dagilim);
public record RaporDto(
    DateTime Baslangic, DateTime Bitis, string Kapsam,
    RaporOzet Ozet, RaporOzet? OncekiOzet, List<TrendNoktasi> Trend, List<DepartmanSatiri> Departmanlar, List<SoruSatiri> Sorular,
    EnpsOzet? Enps = null, double? OncekiEnps = null, IsiHaritasi? Isi = null);
public record CevapSatiri(int CevapId, DateTime Tarih, string? Kisi, string? Departman, string Paket, string Soru, SoruTipi Tip, int Deger, double Skor, string? Yorum, bool Dusuk);
public record RaporFiltre(DateTime? Baslangic, DateTime? Bitis, string? Departman, int? PaketId);
public record DemoVeriSonucu(int Gonderim, int Silinen);

// ---- Kişi geçmişi (İK)
public record KisiNoktasi(DateTime Tarih, string Paket, double Skor, bool Dusuk);
public record KisiGecmisCevap(DateTime Tarih, string Paket, string Soru, SoruTipi Tip, int Deger, double Skor, string? Yorum, bool Dusuk);
public record KisiGecmisiDto(int KullaniciId, string Ad, string Email, string? Departman, double? OrtalamaSkor, bool Dususte, List<KisiNoktasi> Noktalar, List<KisiGecmisCevap> Cevaplar);

// ---- Sistem durumu, denetim, mobil giriş
public record SistemDurumuDto(bool Veritabani, int Son24SaatGonderilen, int Son24SaatHatali, bool SmtpAyarli, bool IkMailAktif, bool WebhookAyarli, bool HatirlatmaAktif, bool MobilGirisAktif, DateTime? SonPersonelSenkron);
public record DenetimDto(int Id, DateTime Zaman, string Kullanici, string Eylem, string? Detay);
public record MobilGirisIstek(string Email);
public record MobilGirisYanit(string Url, string Token);

// ---- Profil dili ve test maili
public record ProfilDilIstek(string Dil);
public record TestMailIstek(string? Adres);
/// <param name="Sandbox">true: SMTP sahte test sunucusu (Mailpit); mail gerçek gelen kutusuna gitmez.</param>
public record TestMailYanit(bool Basarili, string Mesaj, string Sunucu, bool Sandbox);

// ---- eNPS ve ısı haritası
/// <param name="Skor">Destekleyen% − Eleştiren% (−100..100). Destekleyen 9-10, pasif 7-8, eleştiren 1-6.</param>
public record EnpsOzet(int Toplam, int Destekleyen, int Pasif, int Elestiren, double? Skor);
public record IsiHaritasiSatiri(string Departman, List<double?> Degerler, List<int> Sayilar);
public record IsiHaritasi(List<DateTime> Haftalar, List<IsiHaritasiSatiri> Satirlar);

// ---- Aksiyon takibi
public record AksiyonDto(int Id, string Baslik, string? Aciklama, AksiyonDurumu Durum, AksiyonOnceligi Oncelik, int? SorumluId, string? SorumluAd,
    DateTime? SonTarih, bool Gecikmis, int? UyariId, int? KullaniciId, string? KullaniciAd, DateTime Olusturma, DateTime? Kapanma, string? Not);
public record AksiyonKaydetIstek(string? Baslik, string? Aciklama, AksiyonDurumu Durum, AksiyonOnceligi Oncelik, int? SorumluId, DateTime? SonTarih, int? UyariId, int? KullaniciId, string? Not);
public record AksiyonOzetDto(int Acik, int Devam, int Tamam, int Gecikmis);

// ---- KVKK, saklama, marka
public record KvkkAyarDto(int SaklamaAy, string AydinlatmaTr, string AydinlatmaEn);
public record SaklamaOnizleme(int SaklamaAy, DateTime? KesimTarihi, int Gonderim, int Cevap);
public record SaklamaSonucu(int Gonderim, int Cevap);
public record KisiSilSonucu(int Gonderim, int Cevap, int Uyari, int Aksiyon);
public record MarkaDto(string Ad, string Renk);

