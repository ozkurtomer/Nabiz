import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HedefTipi, Kullanici, Paket, Parti } from '../../core/models/models';
import { GonderimService } from '../../core/services/gonderim.service';
import { KullaniciService } from '../../core/services/kullanici.service';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { PaketService } from '../../core/services/paket.service';
import { skorSinifi } from '../../core/utils/cevap';
import { HedefSecici } from '../../shared/hedef-secici/hedef-secici';

@Component({
  selector: 'app-gonderimler',
  imports: [FormsModule, DatePipe, RouterLink, HedefSecici],
  templateUrl: './gonderimler.html',
  styleUrl: './gonderimler.scss',
})
export class Gonderimler implements OnInit {
  private api = inject(GonderimService);
  private paketApi = inject(PaketService);
  private kullaniciApi = inject(KullaniciService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);
  private router = inject(Router);

  readonly liste = signal<Parti[]>([]);
  readonly paketler = signal<Paket[]>([]);
  readonly kullanicilar = signal<Kullanici[]>([]);
  readonly gonderiliyor = signal(false);
  readonly skorSinifi = skorSinifi;

  paketId = 0;
  readonly hedefTipi = signal<HedefTipi>('Hepsi');
  readonly hedefDeger = signal('');
  readonly secilenPaket = computed(() => this.paketler().find(p => p.id === this.paketId));

  ngOnInit() {
    this.yukle();
    this.paketApi.liste().subscribe(p => { const a = p.filter(x => x.aktif); this.paketler.set(a); if (a.length) this.paketId = a[0].id; });
    this.kullaniciApi.liste().subscribe(k => this.kullanicilar.set(k));
  }
  yukle() { this.api.liste().subscribe(l => this.liste.set(l)); }

  async gonder() {
    const p = this.secilenPaket(); if (!p) return;
    const hedef = this.hedefTipi() === 'Hepsi' ? 'tüm çalışanlara' : this.hedefTipi() === 'Departman' ? `${this.hedefDeger()} departmanına` : 'seçilen kişilere';
    if (!await this.onay.sor({ baslik: 'Paket şimdi gönderilsin mi?', mesaj: `"${p.ad}" paketi ${hedef} gönderilecek. E-postalar hemen gider.`, evet: '✉ Şimdi gönder', ikon: '🚀' })) return;
    this.gonderiliyor.set(true);
    this.api.anlik({ paketId: p.id, hedefTipi: this.hedefTipi(), hedefDeger: this.hedefTipi() === 'Hepsi' ? null : this.hedefDeger() }).subscribe({
      next: r => {
        this.gonderiliyor.set(false);
        this.notify.bilgi(`${r.sonuc.basarili}/${r.sonuc.toplam} e-posta gönderildi.`);
        if (r.sonuc.hatali) this.notify.hata(`${r.sonuc.hatali} e-posta gitmedi: ${r.sonuc.ilkHata}`);
        this.router.navigate(['/gonderimler', r.partiId]);
      },
      error: () => this.gonderiliyor.set(false),
    });
  }
}
