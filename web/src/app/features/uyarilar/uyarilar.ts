import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Sayfalama } from '../../shared/sayfalama/sayfalama';
import { AksiyonService } from '../../core/services/aksiyon.service';
import { Uyari, UyariDurumu } from '../../core/models/models';
import { I18n } from '../../core/services/i18n';
import { NotifyService } from '../../core/services/notify.service';
import { UyariService } from '../../core/services/uyari.service';
import { TPipe } from '../../shared/t.pipe';
import { Ikon } from '../../shared/ikon/ikon';

@Component({
  selector: 'app-uyarilar',
  imports: [Ikon, TPipe, FormsModule, DatePipe, RouterLink, Sayfalama],
  templateUrl: './uyarilar.html',
  styleUrl: './uyarilar.scss',
})
export class Uyarilar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(UyariService);
  private notify = inject(NotifyService);
  private aksiyonApi = inject(AksiyonService);
  private router = inject(Router);
  readonly sayfa = signal(1);

  readonly liste = signal<Uyari[]>([]);
  readonly filtre = signal<UyariDurumu | 'Hepsi'>('Hepsi');
  readonly yukleniyor = signal(true);

  readonly gorunen = computed(() => this.filtre() === 'Hepsi' ? this.liste() : this.liste().filter(u => u.durum === this.filtre()));
  readonly sayfadaki = computed(() => this.gorunen().slice((this.sayfa() - 1) * 15, this.sayfa() * 15));
  readonly sayi = (d: UyariDurumu | 'Hepsi') => d === 'Hepsi' ? this.liste().length : this.liste().filter(u => u.durum === d).length;
  readonly sekmeler: { d: UyariDurumu | 'Hepsi'; ad: string }[] = [
    { d: 'Hepsi', ad: 'Tümü' }, { d: 'Yeni', ad: 'Yeni' }, { d: 'Takipte', ad: 'Takipte' }, { d: 'Kapandi', ad: 'Kapandı' },
  ];

  ngOnInit() { this.api.liste().subscribe({ next: l => { this.liste.set(l); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) }); }

  kaydet(u: Uyari) {
    this.api.guncelle(u.id, u.durum, u.ikNotu).subscribe(() => { this.liste.update(l => [...l]); this.notify.bilgi('Uyarı güncellendi.'); });
  }
  /** Uyarıdan aksiyon üretir; uyarı otomatik "Takipte" olur. */
  aksiyonOlustur(u: Uyari) {
    this.aksiyonApi.ekle({ baslik: this.i18n.f('{0} ile görüşme / destek planı', u.kullaniciAd), durum: 'Acik', oncelik: u.sonDonemUyariSayisi >= 3 ? 'Yuksek' : 'Orta', uyariId: u.id, kullaniciId: u.kullaniciId })
      .subscribe(() => { this.notify.bilgi('Aksiyon oluşturuldu.'); this.router.navigate(['/aksiyonlar']); });
  }
  rozet(d: UyariDurumu) { return d === 'Yeni' ? 'hata' : d === 'Takipte' ? 'uyari' : 'ok'; }
}
