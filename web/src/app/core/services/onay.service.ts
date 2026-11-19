import { Injectable, signal } from '@angular/core';

export interface OnayAyar {
  baslik: string;
  mesaj?: string;
  /** Onay düğmesinin yazısı (varsayılan "Evet"). */
  evet?: string;
  /** Vazgeç düğmesinin yazısı (varsayılan "Vazgeç"). */
  hayir?: string;
  /** Silme gibi geri alınamaz işlemlerde kırmızı düğme için 'tehlike'. */
  tur?: 'normal' | 'tehlike';
  ikon?: string;
}

export interface AcikOnay extends OnayAyar { coz: (sonuc: boolean) => void; }

/** Tarayıcının yerleşik confirm() penceresi yerine uygulama tasarımına uygun onay penceresi. */
@Injectable({ providedIn: 'root' })
export class OnayService {
  readonly acik = signal<AcikOnay | null>(null);

  /** Kullanıcı onaylarsa true, vazgeçerse false döner. */
  sor(ayar: OnayAyar): Promise<boolean> {
    // Açık bir onay varsa önce onu "vazgeç" olarak kapat
    this.acik()?.coz(false);
    return new Promise<boolean>(coz => this.acik.set({ ...ayar, coz }));
  }

  kapat(sonuc: boolean) {
    const o = this.acik();
    if (!o) return;
    this.acik.set(null);
    o.coz(sonuc);
  }
}
