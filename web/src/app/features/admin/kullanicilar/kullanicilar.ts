import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Sayfalama } from '../../../shared/sayfalama/sayfalama';
import { Ikon } from '../../../shared/ikon/ikon';
import { FormsModule } from '@angular/forms';
import { Kullanici } from '../../../core/models/models';
import { I18n } from '../../../core/services/i18n';
import { KullaniciService } from '../../../core/services/kullanici.service';
import { NotifyService } from '../../../core/services/notify.service';
import { TPipe } from '../../../shared/t.pipe';

@Component({
  selector: 'app-kullanicilar',
  imports: [TPipe, FormsModule, RouterLink, Sayfalama, Ikon],
  templateUrl: './kullanicilar.html',
})
export class Kullanicilar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(KullaniciService);
  private notify = inject(NotifyService);

  readonly liste = signal<Kullanici[]>([]);
  readonly yukleniyor = signal(true);
  ara = '';
  readonly sayfa = signal(1);
  readonly gorunen = computed(() => this.liste().slice((this.sayfa() - 1) * 25, this.sayfa() * 25));

  ngOnInit() { this.yukle(); }
  yukle() { this.api.liste(this.ara || undefined).subscribe({ next: l => { this.liste.set(l); this.sayfa.set(1); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) }); }

  kaydet(k: Kullanici) {
    this.api.guncelle(k.id, { rol: k.rol, aktif: k.aktif, dil: k.dil }).subscribe(() => this.notify.bilgi('{0} güncellendi.', k.ad));
  }
  aktar() {
    this.api.personelAktar().subscribe(r => {
      this.notify.bilgi('Senkron tamam: {0} yeni, {1} güncel, {2} atlandı.', r.yeni, r.guncel, r.atlanan);
      this.yukle();
    });
  }
}
