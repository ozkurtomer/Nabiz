import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HedefTipi, Kullanici, Paket, Parti, Plan } from '../../core/models/models';
import { GonderimService } from '../../core/services/gonderim.service';
import { KullaniciService } from '../../core/services/kullanici.service';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { PaketService } from '../../core/services/paket.service';
import { PlanService } from '../../core/services/plan.service';
import { skorSinifi } from '../../core/utils/cevap';
import { HedefSecici } from '../../shared/hedef-secici/hedef-secici';

/** Ne zaman gönderilecek: hemen / bir kez (zamanlı) / tekrarlayan. */
type Mod = 'hemen' | 'birkez' | 'tekrar';

/** Gönderim oluşturma, zamanlanmış gönderimler ve gönderim geçmişi tek ekranda. */
@Component({
  selector: 'app-gonderimler',
  imports: [FormsModule, DatePipe, RouterLink, HedefSecici],
  templateUrl: './gonderimler.html',
  styleUrl: './gonderimler.scss',
})
export class Gonderimler implements OnInit {
  private api = inject(GonderimService);
  private planApi = inject(PlanService);
  private paketApi = inject(PaketService);
  private kullaniciApi = inject(KullaniciService);
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
  paketId = 0;
  readonly hedefTipi = signal<HedefTipi>('Hepsi');
  readonly hedefDeger = signal('');
  baslangic = this.bugun();
  saat = '09:00';
  siklikGun = 1;

  readonly secilenPaket = computed(() => this.paketler().find(p => p.id === this.paketId));
  readonly hedefHazir = computed(() => this.hedefTipi() === 'Hepsi' || !!this.hedefDeger());
  readonly bekleyenPlanlar = computed(() => this.planlar().filter(p => p.aktif || p.siklikGun > 0));

  ngOnInit() {
    this.yukle();
    this.paketApi.liste().subscribe(p => { const a = p.filter(x => x.aktif); this.paketler.set(a); if (a.length) this.paketId = a[0].id; });
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
    return this.hedefTipi() === 'Hepsi' ? 'tüm çalışanlara' : this.hedefTipi() === 'Departman' ? `${this.hedefDeger()} departmanına` : 'seçilen kişilere';
  }

  async gonder() {
    const p = this.secilenPaket(); if (!p || !this.hedefHazir()) return;
    const hedef = this.hedefMetni();
    const deger = this.hedefTipi() === 'Hepsi' ? null : this.hedefDeger();
    const tarih = new Date(`${this.baslangic}T${this.saat}`).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });

    if (this.mod() === 'hemen') {
      if (!await this.onay.sor({ baslik: 'Paket şimdi gönderilsin mi?', mesaj: `"${p.ad}" paketi ${hedef} gönderilecek. E-postalar hemen gider.`, evet: '✉ Şimdi gönder', ikon: '🚀' })) return;
      this.gonderiliyor.set(true);
      this.api.anlik({ paketId: p.id, hedefTipi: this.hedefTipi(), hedefDeger: deger }).subscribe({
        next: r => {
          this.gonderiliyor.set(false);
          this.notify.bilgi(`${r.sonuc.basarili}/${r.sonuc.toplam} e-posta gönderildi.`);
          if (r.sonuc.hatali) this.notify.hata(`${r.sonuc.hatali} e-posta gitmedi: ${r.sonuc.ilkHata}`);
          this.router.navigate(['/gonderimler', r.partiId]);
        },
        error: () => this.gonderiliyor.set(false),
      });
      return;
    }

    const birKez = this.mod() === 'birkez';
    const ozet = birKez ? `${tarih} tarihinde bir kez` : `${tarih} itibarıyla ${this.siklikGun === 1 ? 'her gün' : this.siklikGun + ' günde bir'}`;
    if (!await this.onay.sor({
      baslik: birKez ? 'Gönderim zamanlansın mı?' : 'Tekrarlayan gönderim kurulsun mu?',
      mesaj: `"${p.ad}" paketi ${hedef}, ${ozet} gönderilecek.${this.gecmisZaman() ? ' Seçilen zaman geçtiği için ilk gönderim hemen yapılacak.' : ''}`,
      evet: birKez ? '🕘 Zamanla' : '🔁 Kur', ikon: birKez ? '🕘' : '🔁',
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
    if (!await this.onay.sor({ baslik: 'Şimdi gönderilsin mi?', mesaj: `"${p.paketAd}" paketi → ${p.hedefAciklama}. E-postalar hemen gider.`, evet: '✉ Şimdi gönder', ikon: '✉️' })) return;
    this.planApi.simdiGonder(p.id).subscribe(r => {
      this.notify.bilgi(`${r.basarili}/${r.toplam} e-posta gönderildi.`);
      if (r.hatali) this.notify.hata(`${r.hatali} e-posta gitmedi: ${r.ilkHata}`);
      this.yukle();
    });
  }

  async planSil(p: Plan) {
    if (!await this.onay.sor({ baslik: p.siklikGun === 0 && p.aktif ? 'Zamanlı gönderim iptal edilsin mi?' : 'Plan silinsin mi?', mesaj: `"${p.paketAd}" · ${p.hedefAciklama}`, evet: 'Evet, sil', tur: 'tehlike' })) return;
    this.planApi.sil(p.id).subscribe(() => { this.notify.bilgi('Plan silindi.'); this.yukle(); });
  }

  private bugun() { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`; }
}
