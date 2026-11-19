import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Kullanici } from '../../../core/models/models';
import { I18n } from '../../../core/services/i18n';
import { KullaniciService } from '../../../core/services/kullanici.service';
import { NotifyService } from '../../../core/services/notify.service';

@Component({
  selector: 'app-kullanicilar',
  imports: [FormsModule],
  templateUrl: './kullanicilar.html',
})
export class Kullanicilar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(KullaniciService);
  private notify = inject(NotifyService);

  readonly liste = signal<Kullanici[]>([]);
  readonly yukleniyor = signal(true);
  ara = '';

  ngOnInit() { this.yukle(); }
  yukle() { this.api.liste(this.ara || undefined).subscribe({ next: l => { this.liste.set(l); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) }); }

  kaydet(k: Kullanici) {
    this.api.guncelle(k.id, { rol: k.rol, aktif: k.aktif, dil: k.dil }).subscribe(() => this.notify.bilgi(`${k.ad} güncellendi.`));
  }
  aktar() {
    this.api.personelAktar().subscribe(r => {
      this.notify.bilgi(`Senkron tamam: ${r.yeni} yeni, ${r.guncel} güncel, ${r.atlanan} atlandı.`);
      this.yukle();
    });
  }
}
