import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Ayarlar as AyarlarModel } from '../../../core/models/models';
import { AyarService } from '../../../core/services/ayar.service';
import { I18n } from '../../../core/services/i18n';
import { NotifyService } from '../../../core/services/notify.service';
import { OnayService } from '../../../core/services/onay.service';
import { RaporService } from '../../../core/services/rapor.service';
import { SistemService } from '../../../core/services/sistem.service';
import { SistemDurumu } from '../../../core/models/models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-ayarlar',
  imports: [FormsModule, DatePipe],
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
  readonly demoMesgul = signal(false);

  a: AyarlarModel = { emojiEsik: 2, puanEsik: 4, minGrupBuyuklugu: 3 };

  ngOnInit() { this.api.getir().subscribe(r => (this.a = r)); this.sistem.durum().subscribe(d => this.durum.set(d)); }
  kaydet() { this.api.kaydet(this.a).subscribe(() => this.notify.bilgi('Ayarlar kaydedildi.')); }

  demoUret() {
    this.demoMesgul.set(true);
    this.rapor.demoUret().subscribe({
      next: r => { this.notify.bilgi(`${r.gonderim} demo gönderim üretildi. Raporlar sayfasına bakabilirsin.`); this.demoMesgul.set(false); },
      error: () => this.demoMesgul.set(false),
    });
  }
  async demoTemizle() {
    if (!await this.onay.sor({ baslik: 'Demo veriler silinsin mi?', mesaj: 'Yalnızca demo (sahte) gönderimler silinir. Gerçek cevaplara dokunulmaz.', evet: 'Evet, sil', tur: 'tehlike' })) return;
    this.rapor.demoTemizle().subscribe(r => this.notify.bilgi(`${r.silinen} demo gönderim partisi silindi.`));
  }
}
