import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Soru, SoruKaydet } from '../../../core/models/models';
import { I18n } from '../../../core/services/i18n';
import { NotifyService } from '../../../core/services/notify.service';
import { SoruService } from '../../../core/services/soru.service';

@Component({
  selector: 'app-sorular',
  imports: [FormsModule],
  templateUrl: './sorular.html',
  styleUrl: './sorular.scss',
})
export class Sorular implements OnInit {
  readonly i18n = inject(I18n);
  private api = inject(SoruService);
  private notify = inject(NotifyService);

  readonly liste = signal<Soru[]>([]);
  yeni: SoruKaydet = this.bos();

  ngOnInit() { this.yukle(); }
  yukle() { this.api.liste().subscribe(l => this.liste.set(l)); }

  kaydet(s: Soru) { this.api.guncelle(s.id, s).subscribe(() => this.notify.bilgi('Soru güncellendi.')); }
  ekle() { this.api.ekle(this.yeni).subscribe(() => { this.yeni = this.bos(); this.yukle(); }); }

  private bos(): SoruKaydet { return { metinTr: '', metinEn: '', tip: 'Emoji', aktif: true, takipMetniTr: '', takipMetniEn: '' }; }
}
