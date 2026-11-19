import { Component, computed, input, model } from '@angular/core';
import { Ikon } from '../ikon/ikon';
import { TPipe } from '../t.pipe';

/** İstemci tarafı sayfalama çubuğu. Kullanım: <app-sayfalama [toplam]="n" [boyut]="25" [(sayfa)]="sayfa" /> */
@Component({
  selector: 'app-sayfalama',
  imports: [Ikon, TPipe],
  template: `
    @if (toplam() > boyut()) {
      <div class="cubuk">
        <small>{{ bas() }}–{{ bit() }} / {{ toplam() }}</small>
        <span class="dugme">
          <button class="kucuk" [disabled]="sayfa() <= 1" (click)="sayfa.set(sayfa() - 1)" [attr.aria-label]="'Önceki' | t"><app-ikon ad="arrow-left" /></button>
          <small>{{ sayfa() }} / {{ sayfaSayisi() }}</small>
          <button class="kucuk" [disabled]="sayfa() >= sayfaSayisi()" (click)="sayfa.set(sayfa() + 1)" [attr.aria-label]="'Sonraki' | t"><app-ikon ad="arrow-right" /></button>
        </span>
      </div>
    }`,
  styles: `.cubuk { display: flex; justify-content: space-between; align-items: center; padding: 12px 4px; color: var(--soluk); }
    .dugme { display: inline-flex; align-items: center; gap: 10px; }`,
})
export class Sayfalama {
  readonly toplam = input.required<number>();
  readonly boyut = input(25);
  readonly sayfa = model(1);
  readonly sayfaSayisi = computed(() => Math.max(1, Math.ceil(this.toplam() / this.boyut())));
  readonly bas = computed(() => (this.sayfa() - 1) * this.boyut() + 1);
  readonly bit = computed(() => Math.min(this.sayfa() * this.boyut(), this.toplam()));
}
