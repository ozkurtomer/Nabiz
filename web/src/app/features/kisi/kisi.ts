import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { KisiGecmisi, TrendNoktasi } from '../../core/models/models';
import { RaporService } from '../../core/services/rapor.service';
import { cevapGoster, skorSinifi } from '../../core/utils/cevap';
import { TrendGrafik } from '../../shared/trend-grafik/trend-grafik';

/** Bir çalışanın zaman içindeki skorları (yalnızca İK; her görüntüleme denetim kaydına işlenir). */
@Component({
  selector: 'app-kisi',
  imports: [DatePipe, RouterLink, TrendGrafik],
  templateUrl: './kisi.html',
  styleUrl: './kisi.scss',
})
export class Kisi implements OnInit {
  private api = inject(RaporService);

  /** Rota parametresi (withComponentInputBinding). */
  readonly id = input.required<string>();
  readonly gecmis = signal<KisiGecmisi | null>(null);
  readonly hata = signal(false);
  readonly skorSinifi = skorSinifi;
  readonly cevapGoster = cevapGoster;

  readonly trend = computed<TrendNoktasi[]>(() => (this.gecmis()?.noktalar ?? []).map(n => ({ gun: n.tarih, ortalamaSkor: n.skor, cevapSayisi: 1 })));

  ngOnInit() { this.api.kisiGecmisi(+this.id()).subscribe({ next: g => this.gecmis.set(g), error: () => this.hata.set(true) }); }
}
