import { SoruTipi } from '../../core/models/models';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { KvkkService } from '../../core/services/kvkk.service';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { indir } from '../../core/utils/indir';
import { KisiGecmisi, TrendNoktasi } from '../../core/models/models';
import { RaporService } from '../../core/services/rapor.service';
import { cevapGoster as cevapGosterFn, skorSinifi } from '../../core/utils/cevap';
import { TrendGrafik } from '../../shared/trend-grafik/trend-grafik';
import { TPipe } from '../../shared/t.pipe';
import { I18n } from '../../core/services/i18n';
import { Ikon } from '../../shared/ikon/ikon';

/** Bir çalışanın zaman içindeki skorları (yalnızca İK; her görüntüleme denetim kaydına işlenir). */
@Component({
  selector: 'app-kisi',
  imports: [Ikon, TPipe, DatePipe, RouterLink, TrendGrafik],
  templateUrl: './kisi.html',
  styleUrl: './kisi.scss',
})
export class Kisi implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(RaporService);
  private kvkk = inject(KvkkService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);
  private router = inject(Router);

  /** Rota parametresi (withComponentInputBinding). */
  readonly id = input.required<string>();
  readonly gecmis = signal<KisiGecmisi | null>(null);
  readonly hata = signal(false);
  readonly skorSinifi = skorSinifi;
  readonly cevapGoster = (tip: SoruTipi, deger: number) => cevapGosterFn(tip, deger, this.i18n.dil());

  readonly trend = computed<TrendNoktasi[]>(() => (this.gecmis()?.noktalar ?? []).map(n => ({ gun: n.tarih, ortalamaSkor: n.skor, cevapSayisi: 1 })));

  ngOnInit() { this.api.kisiGecmisi(+this.id()).subscribe({ next: g => this.gecmis.set(g), error: () => this.hata.set(true) }); }

  disaAktar() { const g = this.gecmis(); if (g) this.kvkk.disaAktar(g.kullaniciId).subscribe(b => indir(b, `nabiz-kisi-${g.kullaniciId}.csv`)); }

  async sil() {
    const g = this.gecmis(); if (!g) return;
    if (!await this.onay.sor({ baslik: 'Kişinin verileri silinsin mi?', mesaj: '{0} adlı kişinin tüm cevapları, uyarıları ve aksiyonları kalıcı olarak silinecek. Bu işlem geri alınamaz.', degerler: [g.ad], evet: 'Kalıcı sil', tur: 'tehlike' })) return;
    this.kvkk.kisiSil(g.kullaniciId).subscribe(r => { this.notify.bilgi('{0} cevap silindi.', r.cevap); this.router.navigate(['/uyarilar']); });
  }
}
