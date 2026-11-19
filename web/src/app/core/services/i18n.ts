import { Injectable, signal } from '@angular/core';

type Dil = 'tr' | 'en';

const SOZLUK: Record<Dil, Record<string, string>> = {
  tr: {
    giris: 'Giriş yap', email: 'E-posta', sifre: 'Şifre', cikis: 'Çıkış', girisHata: 'E-posta veya şifre hatalı.',
    kaydet: 'Kaydet', tamamla: 'Tamamla', yorum: 'Ne oldu? (isteğe bağlı)', evet: 'Evet', hayir: 'Hayır',
    tamamlandi: 'Cevabın tamamlandı, teşekkürler!', kullanicilar: 'Kullanıcılar', sorular: 'Sorular',
    planlar: 'Planlar', ayarlar: 'Ayarlar', uyarilar: 'Uyarılar', yukleniyor: 'Yükleniyor…',
  },
  en: {
    giris: 'Sign in', email: 'Email', sifre: 'Password', cikis: 'Sign out', girisHata: 'Invalid email or password.',
    kaydet: 'Save', tamamla: 'Complete', yorum: 'What happened? (optional)', evet: 'Yes', hayir: 'No',
    tamamlandi: 'Your answer is complete, thank you!', kullanicilar: 'Users', sorular: 'Questions',
    planlar: 'Schedules', ayarlar: 'Settings', uyarilar: 'Alerts', yukleniyor: 'Loading…',
  },
};

@Injectable({ providedIn: 'root' })
export class I18n {
  readonly dil = signal<Dil>(this.oku());

  t(anahtar: string) { return SOZLUK[this.dil()][anahtar] ?? anahtar; }
  degistir(d: Dil) { this.dil.set(d); try { localStorage.setItem('nabiz.dil', d); } catch { /* yoksay */ } }

  private oku(): Dil {
    try { return localStorage.getItem('nabiz.dil') === 'en' ? 'en' : 'tr'; } catch { return 'tr'; }
  }
}
