import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Denetim as DenetimKaydi } from '../../core/models/models';
import { SistemService } from '../../core/services/sistem.service';
import { TPipe } from '../../shared/t.pipe';

/** Kişi bazlı veriye kim ne zaman baktı, hangi kritik işlem yapıldı. */
@Component({
  selector: 'app-denetim',
  imports: [DatePipe, TPipe],
  template: `
    <div class="sayfa-baslik">
      <div><h2>{{ 'Denetim kaydı' | t }}</h2><p>{{ 'Kişi bazlı verilere erişim ve kritik işlemler burada saklanır. Son 300 kayıt gösterilir.' | t }}</p></div>
    </div>
    <div class="tablo-kutu">
      <table>
        <thead><tr><th>{{ 'Zaman' | t }}</th><th>{{ 'Kim' | t }}</th><th>{{ 'İşlem' | t }}</th><th>{{ 'Detay' | t }}</th></tr></thead>
        <tbody>
          @for (d of liste(); track d.id) {
            <tr><td>{{ d.zaman | date: 'dd.MM.yyyy HH:mm:ss' }}</td><td><b>{{ d.kullanici }}</b></td><td><span class="rozet">{{ d.eylem | t }}</span></td><td>{{ d.detay }}</td></tr>
          } @empty { <tr><td colspan="4" class="bos">{{ 'Henüz kayıt yok.' | t }}</td></tr> }
        </tbody>
      </table>
    </div>`,
})
export class DenetimSayfasi implements OnInit {
  private api = inject(SistemService);
  readonly liste = signal<DenetimKaydi[]>([]);
  ngOnInit() { this.api.denetim().subscribe(l => this.liste.set(l)); }
}
