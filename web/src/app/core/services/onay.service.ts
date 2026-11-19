import { Injectable, inject, signal } from '@angular/core';
import { I18n } from './i18n';

export interface OnayAyar {
  baslik: string;
  mesaj?: string;
  /** Mesajdaki {0}, {1} yer tutucularının değerleri. */
  degerler?: unknown[];
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
  private i18n = inject(I18n);
  readonly acik = signal<AcikOnay | null>(null);

  /** Kullanıcı onaylarsa true, vazgeçerse false döner. */
  sor(ayar: OnayAyar): Promise<boolean> {
    // Açık bir onay varsa önce onu "vazgeç" olarak kapat
    this.acik()?.coz(false);
    const t = this.i18n;
    const cevrilmis: OnayAyar = {
      ...ayar, baslik: t.t(ayar.baslik), mesaj: ayar.mesaj ? t.f(ayar.mesaj, ...(ayar.degerler ?? [])) : undefined,
      evet: ayar.evet ? t.t(ayar.evet) : undefined, hayir: ayar.hayir ? t.t(ayar.hayir) : undefined,
    };
    return new Promise<boolean>(coz => this.acik.set({ ...cevrilmis, coz }));
  }

  kapat(sonuc: boolean) {
    const o = this.acik();
    if (!o) return;
    this.acik.set(null);
    o.coz(sonuc);
  }
}
