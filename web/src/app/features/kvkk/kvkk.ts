import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KvkkAyar, Marka, SaklamaOnizleme } from '../../core/models/models';
import { KvkkService } from '../../core/services/kvkk.service';
import { MarkaService } from '../../core/services/marka.service';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { I18n } from '../../core/services/i18n';
import { Ikon } from '../../shared/ikon/ikon';
import { TPipe } from '../../shared/t.pipe';

/** Veri koruma: saklama süresi, aydınlatma metni, şirket markası. Kişi bazlı dışa aktarma/silme kişi sayfasındadır. */
@Component({
  selector: 'app-kvkk',
  imports: [Ikon, TPipe, FormsModule, DatePipe],
  templateUrl: './kvkk.html',
  styleUrl: './kvkk.scss',
})
export class Kvkk implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(KvkkService);
  private markaApi = inject(MarkaService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);

  ayar: KvkkAyar = { saklamaAy: 24, aydinlatmaTr: '', aydinlatmaEn: '' };
  marka: Marka = { ad: '', renk: '#1f4fc6' };
  readonly onizleme = signal<SaklamaOnizleme | null>(null);
  readonly yuklendi = signal(false);

  ngOnInit() {
    this.api.getir().subscribe(a => { this.ayar = a; this.yuklendi.set(true); });
    this.marka = { ...this.markaApi.marka() };
    this.onizle();
  }

  onizle() { this.api.onizle().subscribe(o => this.onizleme.set(o)); }

  kaydet() { this.api.kaydet(this.ayar).subscribe(() => { this.notify.bilgi('KVKK ayarları kaydedildi.'); this.onizle(); }); }

  markaKaydet() {
    this.markaApi.kaydet(this.marka).subscribe(() => { this.markaApi.uygula(this.marka); this.notify.bilgi('Marka ayarları kaydedildi.'); });
  }

  async temizle() {
    const o = this.onizleme();
    if (!o || o.gonderim === 0) return;
    if (!await this.onay.sor({ baslik: 'Eski veriler silinsin mi?', mesaj: '{0} gönderim ve {1} cevap kalıcı olarak silinecek. Bu işlem geri alınamaz.', degerler: [o.gonderim, o.cevap], evet: 'Kalıcı sil', tur: 'tehlike' })) return;
    this.api.uygula().subscribe(r => { this.notify.bilgi('{0} gönderim silindi.', r.gonderim); this.onizle(); });
  }
}
