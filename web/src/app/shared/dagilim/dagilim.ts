import { Component, computed, inject, input } from '@angular/core';
import { Dagilim, SoruTipi } from '../../core/models/models';
import { EMOJILER } from '../../core/utils/cevap';
import { I18n } from '../../core/services/i18n';
import { TPipe } from '../t.pipe';

/** Cevap değerlerinin dağılımı: her değer için bir çubuk (kaç kişi hangi cevabı seçmiş). */
@Component({
  selector: 'app-dagilim',
  imports: [TPipe],
  templateUrl: './dagilim.html',
  styleUrl: './dagilim.scss',
})
export class DagilimGrafigi {
  private i18n = inject(I18n);
  readonly tip = input.required<SoruTipi>();
  readonly veri = input.required<Dagilim[]>();

  readonly toplam = computed(() => this.veri().reduce((t, d) => t + d.adet, 0));
  readonly cubuklar = computed(() => {
    const en = Math.max(1, ...this.veri().map(d => d.adet)); const t = this.toplam();
    return this.veri().map(d => ({
      ...d, yuzde: t === 0 ? 0 : Math.round(d.adet * 100 / t), yukseklik: d.adet / en * 100,
      etiket: this.etiket(d.deger),
      renk: this.renk(d.deger),
    }));
  });

  private etiket(d: number) {
    const t = this.tip();
    return t === 'Emoji' ? EMOJILER[d - 1] : t === 'EvetHayir' ? this.i18n.t(d === 1 ? 'Evet' : 'Hayır') : String(d);
  }
  /** Olumsuzdan olumluya: kırmızı → sarı → yeşil. */
  private renk(d: number) {
    const t = this.tip();
    const oran = t === 'Emoji' ? (d - 1) / 4 : t === 'Puan' ? (d - 1) / 9 : d;
    return oran < 0.4 ? 'kotu' : oran < 0.7 ? 'orta' : 'iyi';
  }
}
