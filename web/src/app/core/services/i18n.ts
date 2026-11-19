import { Injectable, signal } from '@angular/core';
import { EN } from '../i18n/en';

export type Dil = 'tr' | 'en';

/** Eski kısa anahtarlar (geriye dönük uyumluluk): Türkçe metne çevrilir. */
const KISA: Record<string, string> = {
  giris: 'Giriş yap', email: 'E-posta', sifre: 'Şifre', cikis: 'Çıkış', girisHata: 'E-posta veya şifre hatalı.',
  kaydet: 'Kaydet', tamamla: 'Tamamla', yorum: 'Ne oldu? (isteğe bağlı)', evet: 'Evet', hayir: 'Hayır',
  tamamlandi: 'Cevabın tamamlandı, teşekkürler!', kullanicilar: 'Kullanıcılar', sorular: 'Sorular',
  planlar: 'Planlar', ayarlar: 'Ayarlar', uyarilar: 'Uyarılar', yukleniyor: 'Yükleniyor…',
};

/**
 * Çeviri: kaynak metin Türkçedir ve anahtar olarak kullanılır; İngilizce karşılığı `en.ts` sözlüğündedir.
 * Sözlükte olmayan metin olduğu gibi (Türkçe) gösterilir, böylece eksik çeviri ekranı bozmaz.
 */
@Injectable({ providedIn: 'root' })
export class I18n {
  readonly dil = signal<Dil>(this.oku());

  t(metin: string): string {
    const tr = KISA[metin] ?? metin;
    return this.dil() === 'en' ? (EN[tr] ?? tr) : tr;
  }

  /** Yer tutuculu çeviri: f('{0} kişi seçildi', 3) */
  f(metin: string, ...degerler: unknown[]): string {
    return this.t(metin).replace(/\{(\d+)\}/g, (_, i) => String(degerler[+i] ?? ''));
  }

  /** Kullanıcı açıkça bir dil seçtiyse kalıcı olarak saklar. */
  degistir(d: Dil) { this.dil.set(d); try { localStorage.setItem('nabiz.dil', d); } catch { /* yoksay */ } }

  /** Kullanıcı daha önce dil seçmediyse (ör. e-postadaki cevap linki), önerilen dili kalıcı yapmadan uygular. */
  varsayilan(d: Dil) { if (!this.secildiMi()) this.dil.set(d); }

  secildiMi(): boolean { try { return !!localStorage.getItem('nabiz.dil'); } catch { return false; } }

  private oku(): Dil {
    try { return localStorage.getItem('nabiz.dil') === 'en' ? 'en' : 'tr'; } catch { return 'tr'; }
  }
}
