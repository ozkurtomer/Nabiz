import { Injectable, inject, signal } from '@angular/core';
import { I18n } from './i18n';

export interface Bildirim { id: number; tur: 'bilgi' | 'hata'; mesaj: string; }

/** Bildirimler Türkçe kaynak metinle verilir; seçili dile otomatik çevrilir ({0}, {1} yer tutucuları desteklenir). */
@Injectable({ providedIn: 'root' })
export class NotifyService {
  private i18n = inject(I18n);
  readonly liste = signal<Bildirim[]>([]);
  private sayac = 0;

  bilgi(metin: string, ...degerler: unknown[]) { this.ekle('bilgi', this.i18n.f(metin, ...degerler), 4000); }
  hata(metin: string, ...degerler: unknown[]) { this.ekle('hata', this.i18n.f(metin, ...degerler), 6000); }
  kapat(id: number) { this.liste.update(l => l.filter(x => x.id !== id)); }

  private ekle(tur: Bildirim['tur'], mesaj: string, sureMs: number) {
    const id = ++this.sayac;
    this.liste.update(l => [...l, { id, tur, mesaj }]);
    setTimeout(() => this.kapat(id), sureMs);
  }
}
