import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PartiDetay, PartiKisi } from '../../../core/models/models';
import { GonderimService } from '../../../core/services/gonderim.service';
import { NotifyService } from '../../../core/services/notify.service';
import { cevapGoster, skorSinifi } from '../../../core/utils/cevap';
import { indir } from '../../../core/utils/indir';
import { DagilimGrafigi } from '../../../shared/dagilim/dagilim';
import { TPipe } from '../../../shared/t.pipe';

type Filtre = 'hepsi' | 'cevap' | 'bekleyen' | 'dusuk' | 'hata';

/** Bir gönderimin raporu: kim ne cevap vermiş, kim vermemiş, sorular nasıl dağılmış. */
@Component({
  selector: 'app-gonderim-detay',
  imports: [TPipe, DatePipe, RouterLink, DagilimGrafigi],
  templateUrl: './detay.html',
  styleUrl: './detay.scss',
})
export class GonderimDetay implements OnInit {
  private api = inject(GonderimService);
  private notify = inject(NotifyService);

  /** Rota parametresi (withComponentInputBinding). */
  readonly id = input.required<string>();
  readonly detay = signal<PartiDetay | null>(null);
  readonly filtre = signal<Filtre>('hepsi');
  readonly acik = signal<Set<number>>(new Set());
  readonly skorSinifi = skorSinifi;
  readonly cevapGoster = cevapGoster;

  readonly sekmeler = computed(() => {
    const k = this.detay()?.kisiler ?? [];
    return [
      { f: 'hepsi' as Filtre, ad: 'Tümü', n: k.length },
      { f: 'cevap' as Filtre, ad: 'Cevap verenler', n: k.filter(x => x.tamamlandi).length },
      { f: 'bekleyen' as Filtre, ad: 'Cevap vermeyenler', n: k.filter(x => !x.tamamlandi).length },
      { f: 'dusuk' as Filtre, ad: 'Düşük puanlılar', n: k.filter(x => x.dusuk).length },
      ...(k.some(x => x.gonderimDurumu === 'Hata') ? [{ f: 'hata' as Filtre, ad: 'Mail gitmeyenler', n: k.filter(x => x.gonderimDurumu === 'Hata').length }] : []),
    ];
  });
  readonly gorunen = computed(() => {
    const k = this.detay()?.kisiler ?? [];
    switch (this.filtre()) {
      case 'cevap': return k.filter(x => x.tamamlandi);
      case 'bekleyen': return k.filter(x => !x.tamamlandi);
      case 'dusuk': return k.filter(x => x.dusuk);
      case 'hata': return k.filter(x => x.gonderimDurumu === 'Hata');
      default: return k;
    }
  });

  ngOnInit() { this.yukle(); }
  yukle() { this.api.detay(+this.id()).subscribe(d => this.detay.set(d)); }

  ac(k: PartiKisi) {
    if (!k.tamamlandi) return;
    this.acik.update(s => { const y = new Set(s); y.has(k.kullaniciId) ? y.delete(k.kullaniciId) : y.add(k.kullaniciId); return y; });
  }
  csvIndir() { this.api.csv(+this.id()).subscribe(b => indir(b, `gonderim-${this.id()}.csv`)); }
  tekrarDene() {
    this.api.tekrarDene(+this.id()).subscribe(r => {
      this.notify.bilgi('{0}/{1} e-posta yeniden gönderildi.', r.basarili, r.toplam);
      if (r.hatali) this.notify.hata('{0} e-posta hâlâ gitmiyor: {1}', r.hatali, r.ilkHata);
      this.yukle();
    });
  }
}
