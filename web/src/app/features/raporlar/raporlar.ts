import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CevapSatiri, Paket, Rapor } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { PaketService } from '../../core/services/paket.service';
import { RaporService } from '../../core/services/rapor.service';
import { cevapGoster, skorSinifi } from '../../core/utils/cevap';
import { indir } from '../../core/utils/indir';
import { DagilimGrafigi } from '../../shared/dagilim/dagilim';
import { TrendGrafik } from '../../shared/trend-grafik/trend-grafik';

@Component({
  selector: 'app-raporlar',
  imports: [FormsModule, DatePipe, TrendGrafik, DagilimGrafigi],
  templateUrl: './raporlar.html',
  styleUrl: './raporlar.scss',
})
export class Raporlar implements OnInit {
  readonly auth = inject(AuthService);
  private api = inject(RaporService);
  private paketApi = inject(PaketService);

  readonly rapor = signal<Rapor | null>(null);
  readonly cevaplar = signal<CevapSatiri[]>([]);
  readonly paketler = signal<Paket[]>([]);
  readonly departmanSecenekleri = signal<string[]>([]);
  readonly yukleniyor = signal(true);
  readonly skorSinifi = skorSinifi;
  readonly cevapGoster = cevapGoster;

  hizli = 30;
  baslangic = '';
  bitis = '';
  departman = '';
  paketId = 0;
  sadeceYorumlu = true;

  readonly toplamDuygu = computed(() => { const o = this.rapor()?.ozet; return o ? o.olumlu + o.notr + o.olumsuz : 0; });
  /** Önceki dönemle fark (örn. +4.2); veri yoksa null. */
  fark(simdi?: number | null, onceki?: number | null): number | null {
    return simdi == null || onceki == null ? null : Math.round((simdi - onceki) * 10) / 10;
  }
  csvIndir() {
    this.api.csv({ baslangic: this.baslangic, bitis: this.bitis, departman: this.departman || undefined, paketId: this.paketId || undefined })
      .subscribe(b => indir(b, `nabiz-cevaplar-${this.bitis}.csv`));
  }
  yuzde(n: number) { const t = this.toplamDuygu(); return t ? Math.round(n * 100 / t) : 0; }

  ngOnInit() {
    this.hizliSec(30);
    if (this.auth.rolVar('Admin', 'IK')) this.paketApi.liste().subscribe(p => this.paketler.set(p));
  }

  hizliSec(gun: number) {
    this.hizli = gun;
    const bit = new Date(); const bas = new Date(); bas.setDate(bit.getDate() - (gun - 1));
    this.bitis = this.tarih(bit); this.baslangic = this.tarih(bas);
    this.yukle();
  }
  private tarih(d: Date) { return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`; }

  yukle() {
    const f = { baslangic: this.baslangic, bitis: this.bitis, departman: this.departman || undefined, paketId: this.paketId || undefined };
    this.yukleniyor.set(true);
    this.api.getir(f).subscribe({
      next: r => {
        this.rapor.set(r); this.yukleniyor.set(false);
        if (!this.departman) this.departmanSecenekleri.set(r.departmanlar.filter(d => !d.gizli).map(d => d.departman).sort());
      },
      error: () => this.yukleniyor.set(false),
    });
    this.api.cevaplar(f, this.sadeceYorumlu).subscribe(c => this.cevaplar.set(c));
  }
}
