import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Uyari, UyariDurumu } from '../../core/models/models';
import { I18n } from '../../core/services/i18n';
import { NotifyService } from '../../core/services/notify.service';
import { UyariService } from '../../core/services/uyari.service';
import { TPipe } from '../../shared/t.pipe';
import { Ikon } from '../../shared/ikon/ikon';

@Component({
  selector: 'app-uyarilar',
  imports: [Ikon, TPipe, FormsModule, DatePipe, RouterLink],
  templateUrl: './uyarilar.html',
  styleUrl: './uyarilar.scss',
})
export class Uyarilar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(UyariService);
  private notify = inject(NotifyService);

  readonly liste = signal<Uyari[]>([]);
  readonly filtre = signal<UyariDurumu | 'Hepsi'>('Hepsi');
  readonly yukleniyor = signal(true);

  readonly gorunen = computed(() => this.filtre() === 'Hepsi' ? this.liste() : this.liste().filter(u => u.durum === this.filtre()));
  readonly sayi = (d: UyariDurumu | 'Hepsi') => d === 'Hepsi' ? this.liste().length : this.liste().filter(u => u.durum === d).length;
  readonly sekmeler: { d: UyariDurumu | 'Hepsi'; ad: string }[] = [
    { d: 'Hepsi', ad: 'Tümü' }, { d: 'Yeni', ad: 'Yeni' }, { d: 'Takipte', ad: 'Takipte' }, { d: 'Kapandi', ad: 'Kapandı' },
  ];

  ngOnInit() { this.api.liste().subscribe({ next: l => { this.liste.set(l); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) }); }

  kaydet(u: Uyari) {
    this.api.guncelle(u.id, u.durum, u.ikNotu).subscribe(() => { this.liste.update(l => [...l]); this.notify.bilgi('Uyarı güncellendi.'); });
  }
  rozet(d: UyariDurumu) { return d === 'Yeni' ? 'hata' : d === 'Takipte' ? 'uyari' : 'ok'; }
}
