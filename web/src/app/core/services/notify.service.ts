import { Injectable, signal } from '@angular/core';

export interface Bildirim { id: number; tur: 'bilgi' | 'hata'; mesaj: string; }

@Injectable({ providedIn: 'root' })
export class NotifyService {
  readonly liste = signal<Bildirim[]>([]);
  private sayac = 0;

  bilgi(mesaj: string, sureMs = 4000) { this.ekle('bilgi', mesaj, sureMs); }
  hata(mesaj: string, sureMs = 6000) { this.ekle('hata', mesaj, sureMs); }
  kapat(id: number) { this.liste.update(l => l.filter(x => x.id !== id)); }

  private ekle(tur: Bildirim['tur'], mesaj: string, sureMs: number) {
    const id = ++this.sayac;
    this.liste.update(l => [...l, { id, tur, mesaj }]);
    setTimeout(() => this.kapat(id), sureMs);
  }
}
