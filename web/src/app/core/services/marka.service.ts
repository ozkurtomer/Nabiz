import { Injectable, signal } from '@angular/core';
import { Marka } from '../models/models';
import { ApiBase } from './api.base';

/** Şirket adı ve vurgu rengi (white-label). Giriş yapmadan okunur; CSS değişkenlerine ve sekme başlığına uygulanır. */
@Injectable({ providedIn: 'root' })
export class MarkaService extends ApiBase {
  readonly marka = signal<Marka>({ ad: 'Nabız', renk: '#1f4fc6' });

  constructor() {
    super();
    // Açılışta renk "yanıp sönmesin": son bilinen marka hemen uygulanır, sunucudakiyle güncellenir
    try { const m = JSON.parse(localStorage.getItem('nabiz.marka') ?? 'null'); if (m?.ad && m?.renk) this.uygula(m); } catch { /* yok say */ }
  }

  yukle() {
    this.http.get<Marka>(this.url('marka')).subscribe({ next: m => this.uygula(m), error: () => { /* varsayılan marka */ } });
  }
  uygula(m: Marka) {
    this.marka.set(m);
    try { localStorage.setItem('nabiz.marka', JSON.stringify(m)); } catch { /* yok say */ }
    if (!/^#[0-9a-f]{6}$/i.test(m.renk)) return;
    const st = document.documentElement.style;
    st.setProperty('--vurgu', m.renk);
    st.setProperty('--vurgu2', m.renk);
    st.setProperty('--vurgu-koyu', `color-mix(in srgb, ${m.renk} 78%, #000)`);
    st.setProperty('--vurgu-soft', `color-mix(in srgb, ${m.renk} 10%, #fff)`);
    document.title = m.ad;
  }
  kaydet(m: Marka) { return this.http.put<void>(this.url('marka'), m); }
}
