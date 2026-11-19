export type Rol = 'Calisan' | 'IK';
export type SoruTipi = 'Emoji' | 'Puan' | 'EvetHayir';
export type HedefTipi = 'Hepsi' | 'Departman' | 'Kisi';
export type UyariDurumu = 'Yeni' | 'Takipte' | 'Kapandi';

export interface LoginYanit { token: string; rol: Rol; dil: 'tr' | 'en'; ad: string; }

// ---- Soru ve paket
export interface Soru { id: number; metinTr: string; metinEn: string; tip: SoruTipi; aktif: boolean; takipMetniTr?: string | null; takipMetniEn?: string | null; }
export type SoruKaydet = Omit<Soru, 'id'>;
export interface Paket { id: number; ad: string; aciklama?: string; aktif: boolean; sorular: Soru[]; }
export interface PaketKaydet { ad: string; aciklama?: string | null; aktif: boolean; soruIdleri: number[]; }

// ---- Kullanıcı
export interface Kullanici { id: number; personelId: string; ad: string; email: string; departman?: string; rol: Rol; dil: string; aktif: boolean; }
export interface PersonelAktarimSonucu { yeni: number; guncel: number; atlanan: number; }

// ---- Plan
export interface Plan {
  id: number; paketId: number; paketAd?: string; hedefTipi: HedefTipi; hedefDeger?: string; hedefAciklama: string;
  baslangic: string; siklikGun: number; saat: string; aktif: boolean; sonCalisma?: string;
}
export interface PlanKaydet {
  paketId: number; hedefTipi: HedefTipi; hedefDeger?: string | null; baslangic: string; siklikGun: number; saat: string; aktif: boolean;
}

// ---- Cevaplama
export interface CevapGirdisi { soruId: number; deger: number; yorum?: string | null; anonimYorum?: boolean; }
export interface CevapSayfasi { kullaniciAd: string; paketAd: string; aciklama?: string; sorular: Soru[]; cevaplar: CevapGirdisi[]; tamamlandi: boolean; emojiEsik: number; puanEsik: number; }
export interface CevapKaydetYanit { tamamlandi: boolean; cevaplanan: number; toplam: number; }
export interface BenimSoru { token: string; paket: string; soruSayisi: number; zaman: string; tamamlandi: boolean; }

// ---- Uyarı / ayar
export interface Uyari {
  id: number; olusturma: string; durum: UyariDurumu; ikNotu?: string;
  kullaniciId: number; kullaniciAd: string; departman?: string; soru: string; deger: number; yorum?: string;
  yorumAnonim: boolean; sonDonemUyariSayisi: number;
}
export interface Ayarlar { emojiEsik: number; puanEsik: number; }

// ---- Gönderim
export interface GonderimSonucu { toplam: number; basarili: number; hatali: number; ilkHata?: string; }
export interface AnlikGonder { paketId: number; hedefTipi: HedefTipi; hedefDeger?: string | null; }
export interface Dagilim { deger: number; adet: number; }
export interface Parti {
  id: number; zaman: string; paketId: number; paket: string; soruSayisi: number; hedef: string; anlik: boolean; demo: boolean; test: boolean;
  gonderilen: number; hatali: number; cevaplayan: number; cevapOrani: number; ortalamaSkor?: number;
}
export interface SoruDagilimi { soruId: number; soru: string; tip: SoruTipi; cevapSayisi: number; ortalamaSkor?: number; dagilim: Dagilim[]; }
export interface KisiCevabi { soruId: number; soru: string; tip: SoruTipi; deger: number; skor: number; yorum?: string; yorumAnonim: boolean; dusuk: boolean; }
export interface PartiKisi {
  kullaniciId: number; ad: string; email: string; departman?: string; gonderimDurumu: string; hata?: string;
  tamamlandi: boolean; ortalamaSkor?: number; dusuk: boolean; tamamlanmaZamani?: string; cevaplar: KisiCevabi[];
}
export interface AnonimYorum { soru: string; yorum: string; }
export interface PartiDetay { parti: Parti; sorular: SoruDagilimi[]; kisiler: PartiKisi[]; anonimYorumlar: AnonimYorum[]; }

// ---- Raporlar
export interface RaporOzet { gonderilen: number; cevaplayan: number; cevapOrani: number; ortalamaSkor?: number; olumlu: number; notr: number; olumsuz: number; dusukPuan: number; }
export interface TrendNoktasi { gun: string; ortalamaSkor?: number; cevapSayisi: number; }
export interface DepartmanSatiri { departman: string; gonderilen: number; cevaplayan: number; cevapOrani: number; ortalamaSkor?: number; olumlu: number; notr: number; olumsuz: number; oncekiOrtalamaSkor?: number; }
export interface SoruSatiri { soruId: number; soru: string; tip: SoruTipi; cevapSayisi: number; ortalamaSkor?: number; dagilim: Dagilim[]; }
export interface Rapor {
  baslangic: string; bitis: string; kapsam: string;
  ozet: RaporOzet; oncekiOzet?: RaporOzet; trend: TrendNoktasi[]; departmanlar: DepartmanSatiri[]; sorular: SoruSatiri[];
}
export interface CevapSatiri {
  cevapId: number; tarih: string; kisi?: string; departman?: string; paket: string; soru: string; tip: SoruTipi;
  deger: number; skor: number; yorum?: string; dusuk: boolean;
}
export interface RaporFiltre { baslangic: string; bitis: string; departman?: string; paketId?: number; }
export interface DemoVeriSonucu { gonderim: number; silinen: number; }

// ---- Kişi geçmişi, sistem durumu, denetim
export interface KisiNoktasi { tarih: string; paket: string; skor: number; dusuk: boolean; }
export interface KisiGecmisCevap { tarih: string; paket: string; soru: string; tip: SoruTipi; deger: number; skor: number; yorum?: string; dusuk: boolean; }
export interface KisiGecmisi {
  kullaniciId: number; ad: string; email: string; departman?: string; ortalamaSkor?: number; dususte: boolean;
  noktalar: KisiNoktasi[]; cevaplar: KisiGecmisCevap[];
}
export interface SistemDurumu {
  veritabani: boolean; son24SaatGonderilen: number; son24SaatHatali: number; smtpAyarli: boolean; ikMailAktif: boolean;
  webhookAyarli: boolean; hatirlatmaAktif: boolean; mobilGirisAktif: boolean; sonPersonelSenkron?: string;
}
export interface Denetim { id: number; zaman: string; kullanici: string; eylem: string; detay?: string; }
