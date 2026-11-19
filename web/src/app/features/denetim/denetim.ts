import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { Denetim as DenetimKaydi } from '../../core/models/models';
import { SistemService } from '../../core/services/sistem.service';

/** Kişi bazlı veriye kim ne zaman baktı, hangi kritik işlem yapıldı. */
@Component({
  selector: 'app-denetim',
  imports: [DatePipe],
  template: `
    <div class="sayfa-baslik">
      <div><h2>Denetim kaydı</h2><p>Kişi bazlı verilere erişim ve kritik işlemler burada saklanır. Son 300 kayıt gösterilir.</p></div>
    </div>
    <div class="tablo-kutu">
      <table>
        <thead><tr><th>Zaman</th><th>Kim</th><th>İşlem</th><th>Detay</th></tr></thead>
        <tbody>
          @for (d of liste(); track d.id) {
            <tr><td>{{ d.zaman | date: 'dd.MM.yyyy HH:mm:ss' }}</td><td><b>{{ d.kullanici }}</b></td><td><span class="rozet">{{ d.eylem }}</span></td><td>{{ d.detay }}</td></tr>
          } @empty { <tr><td colspan="4" class="bos">Henüz kayıt yok.</td></tr> }
        </tbody>
      </table>
    </div>`,
})
export class DenetimSayfasi implements OnInit {
  private api = inject(SistemService);
  readonly liste = signal<DenetimKaydi[]>([]);
  ngOnInit() { this.api.denetim().subscribe(l => this.liste.set(l)); }
}
