import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CevapGirdisi, CevapSayfasi } from '../../../core/models/models';
import { AuthService } from '../../../core/services/auth.service';
import { CevapService } from '../../../core/services/cevap.service';
import { I18n } from '../../../core/services/i18n';
import { EMOJILER } from '../../../core/utils/cevap';
import { sec } from '../../../core/utils/dil';
import { Soru } from '../../../core/models/models';
import { Ikon } from '../../../shared/ikon/ikon';
import { TPipe } from '../../../shared/t.pipe';

/** E-postadaki linkten açılır (giriş gerekmez). Paketteki soruları adım adım sorar. */
@Component({
  selector: 'app-cevap',
  imports: [FormsModule, DecimalPipe, TPipe, Ikon],
  templateUrl: './cevap.html',
  styleUrl: './cevap.scss',
})
export class Cevap implements OnInit {
  readonly i18n = inject(I18n);
  readonly auth = inject(AuthService);
  private api = inject(CevapService);
  private router = inject(Router);

  /** Rota parametresi (withComponentInputBinding). */
  readonly token = input.required<string>();

  readonly sayfa = signal<CevapSayfasi | null>(null);
  readonly adim = signal(0);                       // 0 = karşılama, 1..n = sorular
  readonly cevaplar = signal<Record<number, CevapGirdisi>>({});
  readonly tamamlandi = signal(false);
  readonly yukleniyor = signal(true);
  readonly hataVar = signal(false);
  readonly gonderiliyor = signal(false);

  readonly sorular = computed(() => this.sayfa()?.sorular ?? []);
  readonly soru = computed(() => this.adim() > 0 ? this.sorular()[this.adim() - 1] : null);
  readonly mevcut = computed(() => { const s = this.soru(); return s ? this.cevaplar()[s.id] : undefined; });
  /** Cevap düşükse çalışana nazik bir takip sorusu gösterilir (soruya özel metin yoksa genel metin). */
  readonly dusuk = computed(() => {
    const s = this.soru(); const m = this.mevcut(); const p = this.sayfa();
    if (!s || !m || !p) return false;
    return s.tip === 'EvetHayir' ? m.deger === 0 : s.tip === 'Puan' ? m.deger <= p.puanEsik : m.deger <= p.emojiEsik;
  });
  readonly takipMetni = computed(() => {
    const s = this.soru(); const en = this.i18n.dil() === 'en';
    return sec(this.i18n.dil(), s?.takipMetniTr, s?.takipMetniEn) || (en ? 'We are sorry to hear that. Would you like to tell us what happened?' : 'Bunu duyduğumuza üzüldük. Ne olduğunu bizimle paylaşmak ister misin?');
  });
  /** Seçili dilde metin; o dilde yazılmadıysa diğer dile düşer. */
  metin(s: Soru) { return sec(this.i18n.dil(), s.metinTr, s.metinEn); }
  readonly paketAdi = computed(() => { const p = this.sayfa(); return p ? sec(this.i18n.dil(), p.paketAd, p.paketAdEn) : ''; });
  readonly aciklama = computed(() => { const p = this.sayfa(); return p ? sec(this.i18n.dil(), p.aciklama, p.aciklamaEn) : ''; });
  readonly sonAdim = computed(() => this.adim() === this.sorular().length);
  readonly yuzde = computed(() => this.sorular().length ? Math.round((Math.max(this.adim() - 1, 0) / this.sorular().length) * 100) : 0);

  readonly emojiler = [
    { v: 1, s: EMOJILER[0], tr: 'Çok kötü', en: 'Awful' }, { v: 2, s: EMOJILER[1], tr: 'Kötü', en: 'Bad' },
    { v: 3, s: EMOJILER[2], tr: 'İdare eder', en: 'Okay' }, { v: 4, s: EMOJILER[3], tr: 'İyi', en: 'Good' },
    { v: 5, s: EMOJILER[4], tr: 'Harika', en: 'Great' },
  ];
  readonly puanlar = Array.from({ length: 10 }, (_, i) => i + 1);

  ngOnInit() {
    this.api.getir(this.token()).subscribe({
      next: s => {
        this.sayfa.set(s);
        this.i18n.varsayilan(s.dil);   // kullanıcı kendi dilini seçmediyse, kayıtlı dilinde aç
        this.cevaplar.set(Object.fromEntries(s.cevaplar.map(c => [c.soruId, c])));
        this.tamamlandi.set(s.tamamlandi);
        this.yukleniyor.set(false);
      },
      error: () => { this.hataVar.set(true); this.yukleniyor.set(false); },
    });
  }

  sec(deger: number) {
    const s = this.soru(); if (!s) return;
    this.cevaplar.update(c => ({ ...c, [s.id]: { soruId: s.id, deger, yorum: c[s.id]?.yorum ?? null } }));
  }
  yorumYaz(metin: string) {
    const s = this.soru(); const m = this.mevcut(); if (!s || !m) return;
    this.cevaplar.update(c => ({ ...c, [s.id]: { ...m, yorum: metin } }));
  }

  anonimDegistir(v: boolean) {
    const s = this.soru(); const m = this.mevcut(); if (!s || !m) return;
    this.cevaplar.update(c => ({ ...c, [s.id]: { ...m, anonimYorum: v } }));
  }

  basla() { this.adim.set(1); }
  geri() { this.adim.update(a => Math.max(0, a - 1)); }

  ileri() {
    if (!this.mevcut()) return;
    if (this.sonAdim()) { this.gonder(true); return; }
    this.gonder(false);   // ilerlemeyi her adımda kaydet (yarım kalırsa kaybolmasın)
    this.adim.update(a => a + 1);
  }

  private gonder(tamamla: boolean) {
    const liste = Object.values(this.cevaplar());
    if (tamamla) this.gonderiliyor.set(true);
    this.api.kaydet(this.token(), liste, tamamla).subscribe({
      next: r => { if (tamamla) { this.tamamlandi.set(r.tamamlandi); this.gonderiliyor.set(false); } },
      error: () => this.gonderiliyor.set(false),
    });
  }

  panele() { this.router.navigate(['/']); }
}
