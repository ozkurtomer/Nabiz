import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HedefTipi, Kullanici, Paket, Parti, Plan, Soru } from '../../core/models/models';
import { GonderimService } from '../../core/services/gonderim.service';
import { KullaniciService } from '../../core/services/kullanici.service';
import { I18n } from '../../core/services/i18n';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { PaketService } from '../../core/services/paket.service';
import { PlanService } from '../../core/services/plan.service';
import { skorSinifi } from '../../core/utils/cevap';
import { sec } from '../../core/utils/dil';
import { HedefSecici } from '../../shared/hedef-secici/hedef-secici';
import { TPipe } from '../../shared/t.pipe';
import { Ikon } from '../../shared/ikon/ikon';

/** Ne zaman gönderilecek: hemen / bir kez (zamanlı) / tekrarlayan. */
type Mod = 'hemen' | 'birkez' | 'tekrar';

/** Gönderim oluşturma, zamanlanmış gönderimler ve gönderim geçmişi tek ekranda. */
@Component({
  selector: 'app-gonderimler',
  imports: [Ikon, TPipe, FormsModule, DatePipe, RouterLink, HedefSecici],
  templateUrl: './gonderimler.html',
  styleUrl: './gonderimler.scss',
})
export class Gonderimler implements OnInit {
  private api = inject(GonderimService);
  private planApi = inject(PlanService);
  private paketApi = inject(PaketService);
  private kullaniciApi = inject(KullaniciService);
  readonly i18n = inject(I18n);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);
  private router = inject(Router);

  readonly liste = signal<Parti[]>([]);
  readonly planlar = signal<Plan[]>([]);
  readonly paketler = signal<Paket[]>([]);
  readonly kullanicilar = signal<Kullanici[]>([]);
  readonly gonderiliyor = signal(false);
  readonly skorSinifi = skorSinifi;

  readonly mod = signal<Mod>('hemen');
  /** Sinyal olmalı: secilenPaket (computed) seçim değiştikçe yeniden hesaplanır. */
  readonly paketId = signal(0);
  readonly hedefTipi = signal<HedefTipi>('Hepsi');
  readonly hedefDeger = signal('');
  baslangic = this.bugun();
  saat = '09:00';
  siklikGun = 1;

  /** Seçili dilde paket adı / soru metni (o dilde yazılmadıysa diğer dile düşer). */
  ad(p: Paket) { return sec(this.i18n.dil(), p.ad, p.adEn); }
  metin(s: Soru) { return sec(this.i18n.dil(), s.metinTr, s.metinEn); }

  readonly secilenPaket = computed(() => this.paketler().find(p => p.id === this.paketId()));
  readonly hedefHazir = computed(() => this.hedefTipi() === 'Hepsi' || !!this.hedefDeger());

  ngOnInit() {
    this.yukle();
    this.paketApi.liste().subscribe(p => { const a = p.filter(x => x.aktif); this.paketler.set(a); if (a.length) this.paketId.set(a[0].id); });
    this.kullaniciApi.liste().subscribe(k => this.kullanicilar.set(k));
  }
  yukle() { this.api.liste().subscribe(l => this.liste.set(l)); this.planApi.liste().subscribe(p => this.planlar.set(p)); }

  /** Seçilen tarih-saat geçmişse zamanlı gönderim hemen (yaklaşık 1 dakika içinde) yapılır. */
  gecmisZaman(): boolean {
    if (this.mod() === 'hemen' || !this.baslangic) return false;
    const simdi = new Date();
    return new Date(`${this.baslangic}T${this.saat || '00:00'}`) <= simdi;
  }

  private hedefMetni(): string {
    return this.hedefTipi() === 'Hepsi' ? this.i18n.t('tüm çalışanlara') : this.hedefTipi() === 'Departman' ? this.i18n.f('{0} departmanına', this.hedefDeger()) : this.i18n.t('seçilen kişilere');
  }

  async gonder() {
    const p = this.secilenPaket(); if (!p || !this.hedefHazir()) return;
    const hedef = this.hedefMetni();
    const deger = this.hedefTipi() === 'Hepsi' ? null : this.hedefDeger();
    const tarih = new Date(`${this.baslangic}T${this.saat}`).toLocaleString(this.i18n.dil() === 'en' ? 'en-GB' : 'tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });

    if (this.mod() === 'hemen') {
      if (!await this.onay.sor({ baslik: 'Paket şimdi gönderilsin mi?', mesaj: '"{0}" paketi {1} gönderilecek. E-postalar hemen gider.', degerler: [this.ad(p), hedef], evet: 'Şimdi gönder' })) return;
      this.gonderiliyor.set(true);
      this.api.anlik({ paketId: p.id, hedefTipi: this.hedefTipi(), hedefDeger: deger }).subscribe({
        next: r => {
          this.gonderiliyor.set(false);
          this.notify.bilgi('{0}/{1} e-posta gönderildi.', r.sonuc.basarili, r.sonuc.toplam);
          if (r.sonuc.hatali) this.notify.hata('{0} e-posta gitmedi: {1}', r.sonuc.hatali, r.sonuc.ilkHata);
          this.router.navigate(['/gonderimler', r.partiId]);
        },
        error: () => this.gonderiliyor.set(false),
      });
      return;
    }

    const birKez = this.mod() === 'birkez';
    const ozet = birKez
      ? this.i18n.f('{0} tarihinde bir kez', tarih)
      : this.i18n.f('{0} itibarıyla {1}', tarih, this.siklikGun === 1 ? this.i18n.t('her gün') : this.i18n.f('{0} günde bir', this.siklikGun));
    if (!await this.onay.sor({
      baslik: birKez ? 'Gönderim zamanlansın mı?' : 'Tekrarlayan gönderim kurulsun mu?',
      mesaj: '"{0}" paketi {1}, {2} gönderilecek.{3}',
      degerler: [this.ad(p), hedef, ozet, this.gecmisZaman() ? ' ' + this.i18n.t('Seçilen zaman geçtiği için ilk gönderim hemen yapılacak.') : ''],
      evet: birKez ? 'Zamanla' : 'Kur',
    })) return;
    this.gonderiliyor.set(true);
    this.planApi.ekle({
      paketId: p.id, hedefTipi: this.hedefTipi(), hedefDeger: deger, baslangic: this.baslangic,
      siklikGun: birKez ? 0 : this.siklikGun, saat: this.saat, aktif: true,
    }).subscribe({
      next: () => { this.gonderiliyor.set(false); this.notify.bilgi(birKez ? 'Gönderim zamanlandı.' : 'Tekrarlayan gönderim kuruldu.'); this.yukle(); },
      error: () => this.gonderiliyor.set(false),
    });
  }

  async planSimdi(p: Plan) {
    if (!await this.onay.sor({ baslik: 'Şimdi gönderilsin mi?', mesaj: '"{0}" paketi → {1}. E-postalar hemen gider.', degerler: [p.paketAd, p.hedefAciklama], evet: 'Şimdi gönder' })) return;
    this.planApi.simdiGonder(p.id).subscribe(r => {
      this.notify.bilgi('{0}/{1} e-posta gönderildi.', r.basarili, r.toplam);
      if (r.hatali) this.notify.hata('{0} e-posta gitmedi: {1}', r.hatali, r.ilkHata);
      this.yukle();
    });
  }

  async planSil(p: Plan) {
    if (!await this.onay.sor({ baslik: p.siklikGun === 0 && p.aktif ? 'Zamanlı gönderim iptal edilsin mi?' : 'Plan silinsin mi?', mesaj: '"{0}" · {1}', degerler: [p.paketAd, p.hedefAciklama], evet: 'Evet, sil', tur: 'tehlike' })) return;
    this.planApi.sil(p.id).subscribe(() => { this.notify.bilgi('Plan silindi.'); this.yukle(); });
  }

  private bugun() { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`; }
}
