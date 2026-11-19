import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Ayarlar as AyarlarModel } from '../../../core/models/models';
import { AyarService } from '../../../core/services/ayar.service';
import { I18n } from '../../../core/services/i18n';
import { NotifyService } from '../../../core/services/notify.service';
import { OnayService } from '../../../core/services/onay.service';
import { RaporService } from '../../../core/services/rapor.service';
import { SistemService } from '../../../core/services/sistem.service';
import { SistemDurumu, TestMailYanit } from '../../../core/models/models';
import { DatePipe } from '@angular/common';
import { TPipe } from '../../../shared/t.pipe';
import { Ikon } from '../../../shared/ikon/ikon';

@Component({
  selector: 'app-ayarlar',
  imports: [Ikon, TPipe, FormsModule, DatePipe],
  templateUrl: './ayarlar.html',
  styleUrl: './ayarlar.scss',
})
export class Ayarlar implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(AyarService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);
  private rapor = inject(RaporService);
  private sistem = inject(SistemService);
  readonly durum = signal<SistemDurumu | null>(null);
  readonly testSonuc = signal<TestMailYanit | null>(null);
  readonly testBekliyor = signal(false);
  testAdres = '';
  readonly demoMesgul = signal(false);

  a: AyarlarModel = { emojiEsik: 2, puanEsik: 4 };

  ngOnInit() { this.api.getir().subscribe(r => (this.a = r)); this.sistem.durum().subscribe(d => this.durum.set(d)); }
  kaydet() { this.api.kaydet(this.a).subscribe(() => this.notify.bilgi('Ayarlar kaydedildi.')); }

  /** SMTP ayarlarını doğrular: gerçekten bir e-posta gönderir ve sonucu (ya da hata nedenini) gösterir. */
  testMailGonder() {
    this.testBekliyor.set(true); this.testSonuc.set(null);
    this.sistem.testMail(this.testAdres.trim() || undefined).subscribe({
      next: r => { this.testSonuc.set(r); this.testBekliyor.set(false); },
      error: () => this.testBekliyor.set(false),
    });
  }

  demoUret() {
    this.demoMesgul.set(true);
    this.rapor.demoUret().subscribe({
      next: r => { this.notify.bilgi('{0} demo gönderim üretildi. Raporlar sayfasına bakabilirsin.', r.gonderim); this.demoMesgul.set(false); },
      error: () => this.demoMesgul.set(false),
    });
  }
  async demoTemizle() {
    if (!await this.onay.sor({ baslik: 'Demo veriler silinsin mi?', mesaj: 'Yalnızca demo (sahte) gönderimler silinir. Gerçek cevaplara dokunulmaz.', evet: 'Evet, sil', tur: 'tehlike' })) return;
    this.rapor.demoTemizle().subscribe(r => this.notify.bilgi('{0} demo gönderim partisi silindi.', r.silinen));
  }
}
