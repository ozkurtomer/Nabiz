import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HedefTipi, Kullanici, Paket, Plan } from '../../../core/models/models';
import { I18n } from '../../../core/services/i18n';
import { KullaniciService } from '../../../core/services/kullanici.service';
import { NotifyService } from '../../../core/services/notify.service';
import { OnayService } from '../../../core/services/onay.service';
import { PaketService } from '../../../core/services/paket.service';
import { PlanService } from '../../../core/services/plan.service';
import { HedefSecici } from '../../../shared/hedef-secici/hedef-secici';

@Component({
  selector: 'app-planlar',
  imports: [FormsModule, DatePipe, HedefSecici],
  templateUrl: './planlar.html',
  styleUrl: './planlar.scss',
})
export class Planlar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(PlanService);
  private paketApi = inject(PaketService);
  private kullaniciApi = inject(KullaniciService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);

  readonly liste = signal<Plan[]>([]);
  readonly paketler = signal<Paket[]>([]);
  readonly kullanicilar = signal<Kullanici[]>([]);

  paketId = 0;
  readonly hedefTipi = signal<HedefTipi>('Hepsi');
  readonly hedefDeger = signal('');
  baslangic = new Date().toISOString().slice(0, 10);
  siklikGun = 1;
  saat = '09:00';

  ngOnInit() {
    this.yukle();
    this.paketApi.liste().subscribe(p => { const a = p.filter(x => x.aktif); this.paketler.set(a); if (a.length) this.paketId = a[0].id; });
    this.kullaniciApi.liste().subscribe(k => this.kullanicilar.set(k));
  }
  yukle() { this.api.liste().subscribe(p => this.liste.set(p)); }

  ekle() {
    this.api.ekle({
      paketId: this.paketId, hedefTipi: this.hedefTipi(), hedefDeger: this.hedefTipi() === 'Hepsi' ? null : this.hedefDeger(),
      baslangic: this.baslangic, siklikGun: this.siklikGun, saat: this.saat, aktif: true,
    }).subscribe(() => { this.notify.bilgi('Plan eklendi.'); this.yukle(); });
  }
  sil(p: Plan) { this.api.sil(p.id).subscribe(() => { this.notify.bilgi('Plan silindi.'); this.yukle(); }); }

  async simdiGonder(p: Plan) {
    if (!await this.onay.sor({ baslik: 'Plan şimdi gönderilsin mi?', mesaj: `"${p.paketAd}" paketi → ${p.hedefAciklama}. E-postalar hemen gider.`, evet: '✉ Şimdi gönder', ikon: '✉️' })) return;
    this.api.simdiGonder(p.id).subscribe(r => {
      this.notify.bilgi(`${r.basarili}/${r.toplam} e-posta gönderildi.`);
      if (r.hatali) this.notify.hata(`${r.hatali} e-posta gitmedi: ${r.ilkHata}`);
      this.yukle();
    });
  }
}
