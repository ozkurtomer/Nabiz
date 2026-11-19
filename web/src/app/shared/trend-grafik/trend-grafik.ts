import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { TrendNoktasi } from '../../core/models/models';

/** Günlük ortalama skor (0-100) çizgi grafiği; veri olmayan günlerde çizgi kopar. Kütüphanesiz SVG. */
@Component({
  selector: 'app-trend-grafik',
  imports: [DatePipe],
  templateUrl: './trend-grafik.html',
  styleUrl: './trend-grafik.scss',
})
export class TrendGrafik {
  readonly noktalar = input.required<TrendNoktasi[]>();

  readonly G = 640; readonly Y = 220; readonly SOL = 34; readonly SAG = 10; readonly UST = 12; readonly ALT = 26;
  readonly cizgiler = [0, 25, 50, 75, 100];

  private x = (i: number, n: number) => this.SOL + (n <= 1 ? 0 : (i / (n - 1)) * (this.G - this.SOL - this.SAG));
  y = (skor: number) => this.UST + (1 - skor / 100) * (this.Y - this.UST - this.ALT);

  readonly dolu = computed(() => {
    const n = this.noktalar();
    return n.map((p, i) => ({ ...p, cx: this.x(i, n.length), cy: p.ortalamaSkor == null ? null : this.y(p.ortalamaSkor) }));
  });
  /** Ardışık dolu noktalar tek çizgi parçası olur. */
  readonly yollar = computed(() => {
    const parcalar: string[] = []; let simdiki = '';
    for (const p of this.dolu()) {
      if (p.cy === null) { if (simdiki) parcalar.push(simdiki); simdiki = ''; continue; }
      simdiki += `${simdiki ? 'L' : 'M'}${p.cx.toFixed(1)},${p.cy.toFixed(1)} `;
    }
    if (simdiki) parcalar.push(simdiki);
    return parcalar;
  });
  readonly etiketler = computed(() => {
    const n = this.dolu(); const adim = Math.max(1, Math.ceil(n.length / 7));
    return n.filter((_, i) => i % adim === 0);
  });
}
