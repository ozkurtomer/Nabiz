import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Paket, Soru } from '../../core/models/models';
import { AuthService } from '../../core/services/auth.service';
import { CevapService } from '../../core/services/cevap.service';
import { I18n } from '../../core/services/i18n';
import { NotifyService } from '../../core/services/notify.service';
import { OnayService } from '../../core/services/onay.service';
import { PaketService } from '../../core/services/paket.service';
import { SoruService } from '../../core/services/soru.service';

interface PaketForm { id?: number; ad: string; aciklama: string; aktif: boolean; secili: number[]; }

@Component({
  selector: 'app-paketler',
  imports: [FormsModule],
  templateUrl: './paketler.html',
  styleUrl: './paketler.scss',
})
export class Paketler implements OnInit {
  readonly i18n = inject(I18n);
  readonly auth = inject(AuthService);
  private api = inject(PaketService);
  private sorularApi = inject(SoruService);
  private cevap = inject(CevapService);
  private notify = inject(NotifyService);
  private onay = inject(OnayService);
  private router = inject(Router);

  readonly liste = signal<Paket[]>([]);
  readonly havuz = signal<Soru[]>([]);
  readonly form = signal<PaketForm | null>(null);
  readonly ara = signal('');

  readonly secilenSorular = computed(() => {
    const f = this.form(); const h = new Map(this.havuz().map(s => [s.id, s]));
    return (f?.secili ?? []).map(id => h.get(id)).filter((s): s is Soru => !!s);
  });
  readonly eklenebilir = computed(() => {
    const secili = new Set(this.form()?.secili ?? []); const a = this.ara().toLocaleLowerCase('tr');
    return this.havuz().filter(s => !secili.has(s.id) && (!a || s.metinTr.toLocaleLowerCase('tr').includes(a)));
  });

  ngOnInit() { this.yukle(); this.sorularApi.liste().subscribe(s => this.havuz.set(s)); }
  yukle() { this.api.liste().subscribe(p => this.liste.set(p)); }

  yeni() { this.form.set({ ad: '', aciklama: '', aktif: true, secili: [] }); }
  duzenle(p: Paket) { this.form.set({ id: p.id, ad: p.ad, aciklama: p.aciklama ?? '', aktif: p.aktif, secili: p.sorular.map(s => s.id) }); }
  vazgec() { this.form.set(null); }

  ekle(id: number) { this.form.update(f => f && { ...f, secili: [...f.secili, id] }); }
  cikar(id: number) { this.form.update(f => f && { ...f, secili: f.secili.filter(x => x !== id) }); }
  tasi(i: number, yon: -1 | 1) {
    this.form.update(f => {
      if (!f) return f; const s = [...f.secili]; const j = i + yon;
      if (j < 0 || j >= s.length) return f;
      [s[i], s[j]] = [s[j], s[i]];
      return { ...f, secili: s };
    });
  }
  adGuncelle(v: string) { this.form.update(f => f && { ...f, ad: v }); }
  aciklamaGuncelle(v: string) { this.form.update(f => f && { ...f, aciklama: v }); }
  aktifGuncelle(v: boolean) { this.form.update(f => f && { ...f, aktif: v }); }

  kaydet() {
    const f = this.form(); if (!f) return;
    const govde = { ad: f.ad, aciklama: f.aciklama || null, aktif: f.aktif, soruIdleri: f.secili };
    const istek = f.id ? this.api.guncelle(f.id, govde) : this.api.ekle(govde);
    istek.subscribe(() => { this.notify.bilgi('Paket kaydedildi.'); this.form.set(null); this.yukle(); });
  }
  async sil(p: Paket) {
    if (!await this.onay.sor({ baslik: 'Paket silinsin mi?', mesaj: `"${p.ad}" paketi kalıcı olarak silinecek.`, evet: 'Evet, sil', tur: 'tehlike' })) return;
    this.api.sil(p.id).subscribe(() => { this.notify.bilgi('Paket silindi.'); this.yukle(); });
  }
  /** Paketi e-posta atmadan kendine gönderip cevaplama ekranını açar. */
  dene(p: Paket) { this.cevap.testGonderimi(p.id).subscribe(r => this.router.navigate(['/cevap', r.token])); }
}
