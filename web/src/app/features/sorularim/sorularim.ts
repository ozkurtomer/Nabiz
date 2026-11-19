import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BenimSoru } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { CevapService } from '../../core/services/cevap.service';

@Component({
  selector: 'app-sorularim',
  imports: [DatePipe, RouterLink],
  template: `
    <div class="sayfa-baslik">
      <div><h2>Merhaba {{ auth.ad().split(' ')[0] }} 👋</h2><p>Sana gönderilen sorular burada. E-postandaki linkten de cevaplayabilirsin.</p></div>
    </div>
    @if (yukleniyor()) { <p class="bos">Yükleniyor…</p> }
    @else if (liste().length === 0) {
      <div class="kart" style="text-align:center;padding:48px 20px"><div style="font-size:3rem">📭</div><h3>Henüz sana gelen soru yok</h3>
        <p style="color:var(--soluk)">Yeni bir soru gönderildiğinde e-postayla haber vereceğiz.</p></div>
    } @else {
      <div style="display:grid;gap:12px">
        @for (b of liste(); track b.token) {
          <article class="kart" style="display:flex;align-items:center;gap:16px;margin:0;flex-wrap:wrap">
            <div style="flex:1;min-width:200px"><b>{{ b.paket }}</b>
              <small style="display:block">{{ b.soruSayisi }} soru · {{ b.zaman | date: 'dd.MM.yyyy HH:mm' }}</small></div>
            @if (b.tamamlandi) { <span class="rozet ok">✓ Tamamlandı</span> }
            @else { <a [routerLink]="['/cevap', b.token]"><button class="birincil">Cevapla →</button></a> }
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
