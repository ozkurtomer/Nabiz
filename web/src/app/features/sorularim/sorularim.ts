import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BenimSoru } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { CevapService } from '../../core/services/cevap.service';
import { TPipe } from '../../shared/t.pipe';
import { Ikon } from '../../shared/ikon/ikon';

@Component({
  selector: 'app-sorularim',
  imports: [Ikon, DatePipe, RouterLink, TPipe],
  template: `
    <div class="sayfa-baslik">
      <div><h2>{{ 'Merhaba' | t }} {{ auth.ad().split(' ')[0] }} </h2><p>{{ 'Sana gönderilen sorular burada. E-postandaki linkten de cevaplayabilirsin.' | t }}</p></div>
    </div>
    @if (yukleniyor()) { <p class="bos">{{ 'Yükleniyor…' | t }}</p> }
    @else if (liste().length === 0) {
      <div class="kart" style="text-align:center;padding:48px 20px"><div style="width:56px;height:56px;margin:0 auto 12px;border-radius:50%;background:var(--vurgu-soft);color:var(--vurgu);display:grid;place-items:center;--ikon-boyut:26px"><app-ikon ad="inbox" /></div><h3>{{ 'Henüz sana gelen soru yok' | t }}</h3>
        <p style="color:var(--soluk)">{{ 'Yeni bir soru gönderildiğinde e-postayla haber vereceğiz.' | t }}</p></div>
    } @else {
      <div style="display:grid;gap:12px">
        @for (b of liste(); track b.token) {
          <article class="kart" style="display:flex;align-items:center;gap:16px;margin:0;flex-wrap:wrap">
            <div style="flex:1;min-width:200px"><b>{{ b.paket }}</b>
              <small style="display:block">{{ b.soruSayisi }} {{ 'soru' | t }} · {{ b.zaman | date: 'dd.MM.yyyy HH:mm' }}</small></div>
            @if (b.tamamlandi) { <span class="rozet ok"><app-ikon ad="check" /> {{ 'Tamamlandı' | t }}</span> }
            @else { <a [routerLink]="['/cevap', b.token]"><button class="birincil">{{ 'Cevapla →' | t }}</button></a> }
          </article>
        }
      </div>
    }`,
})
export class Sorularim implements OnInit {
  readonly auth = inject(AuthService);
  private api = inject(CevapService);
  readonly liste = signal<BenimSoru[]>([]);
  readonly yukleniyor = signal(true);
  ngOnInit() { this.api.benim().subscribe({ next: l => { this.liste.set(l); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) }); }
}
