import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Aksiyon, AksiyonDurumu, AksiyonKaydet, AksiyonOnceligi, AksiyonOzet, Kullanici } from '../../core/models/models';
import { AksiyonService } from '../../core/services/aksiyon.service';
import { KullaniciService } from '../../core/services/kullanici.service';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { I18n } from '../../core/services/i18n';
import { Ikon } from '../../shared/ikon/ikon';
import { TPipe } from '../../shared/t.pipe';

/** Uyarılardan ve raporlardan doğan iyileştirme görevleri: sorumlu, öncelik, son tarih, durum. */
@Component({
  selector: 'app-aksiyonlar',
  imports: [Ikon, TPipe, FormsModule, DatePipe],
  templateUrl: './aksiyonlar.html',
  styleUrl: './aksiyonlar.scss',
})
export class Aksiyonlar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(AksiyonService);
  private kullaniciApi = inject(KullaniciService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);

  readonly liste = signal<Aksiyon[]>([]);
  readonly ozet = signal<AksiyonOzet>({ acik: 0, devam: 0, tamam: 0, gecikmis: 0 });
  readonly ikler = signal<Kullanici[]>([]);
  readonly filtre = signal<'Aktif' | 'Tamam' | 'Hepsi'>('Aktif');
  readonly yukleniyor = signal(true);
  readonly yeniAcik = signal(false);
  yeni: AksiyonKaydet = this.bos();

  readonly gorunen = computed(() => {
    const f = this.filtre();
    return this.liste().filter(a => f === 'Hepsi' || (f === 'Aktif' ? a.durum === 'Acik' || a.durum === 'Devam' : a.durum === 'Tamam' || a.durum === 'Iptal'));
  });
  readonly sekmeler: { d: 'Aktif' | 'Tamam' | 'Hepsi'; ad: string }[] = [{ d: 'Aktif', ad: 'Aktif' }, { d: 'Tamam', ad: 'Kapanan' }, { d: 'Hepsi', ad: 'Tümü' }];
  readonly durumlar: { d: AksiyonDurumu; ad: string }[] = [{ d: 'Acik', ad: 'Açık' }, { d: 'Devam', ad: 'Devam ediyor' }, { d: 'Tamam', ad: 'Tamamlandı' }, { d: 'Iptal', ad: 'İptal' }];
  readonly oncelikler: { d: AksiyonOnceligi; ad: string }[] = [{ d: 'Yuksek', ad: 'Yüksek' }, { d: 'Orta', ad: 'Orta' }, { d: 'Dusuk', ad: 'Düşük' }];

  ngOnInit() {
    this.yukle();
    this.kullaniciApi.liste().subscribe(k => this.ikler.set(k.filter(x => x.rol === 'IK' && x.aktif)));
  }

  yukle() {
    this.api.liste().subscribe({ next: l => { this.liste.set(l); this.yukleniyor.set(false); }, error: () => this.yukleniyor.set(false) });
    this.api.ozet().subscribe(o => this.ozet.set(o));
  }

  ekle() {
    this.api.ekle({ ...this.yeni, sonTarih: this.yeni.sonTarih || null }).subscribe(() => {
      this.yeni = this.bos(); this.yeniAcik.set(false); this.notify.bilgi('Aksiyon oluşturuldu.'); this.yukle();
    });
  }

  kaydet(a: Aksiyon) {
    this.api.guncelle(a.id, {
      baslik: a.baslik, aciklama: a.aciklama ?? null, durum: a.durum, oncelik: a.oncelik, sorumluId: a.sorumluId ?? null,
      sonTarih: a.sonTarih ? a.sonTarih.substring(0, 10) : null, uyariId: a.uyariId ?? null, kullaniciId: a.kullaniciId ?? null, not: a.not ?? null,
    }).subscribe(() => { this.notify.bilgi('Aksiyon güncellendi.'); this.yukle(); });
  }

  async sil(a: Aksiyon) {
    if (!await this.onay.sor({ baslik: 'Aksiyon silinsin mi?', mesaj: '“{0}” kalıcı olarak silinecek.', degerler: [a.baslik], evet: 'Sil', tur: 'tehlike' })) return;
    this.api.sil(a.id).subscribe(() => { this.notify.bilgi('Aksiyon silindi.'); this.yukle(); });
  }

  private bos(): AksiyonKaydet { return { baslik: '', aciklama: '', durum: 'Acik', oncelik: 'Orta', sorumluId: null, sonTarih: null }; }
}
