import { Component, computed, input, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HedefTipi, Kullanici } from '../../core/models/models';
import { TPipe } from '../t.pipe';

/** Hedef kitle seçici: Tüm çalışanlar / Departman / Kişi (çoklu). Değer, planlarla aynı biçimde tutulur. */
@Component({
  selector: 'app-hedef-secici',
  imports: [TPipe, FormsModule],
  templateUrl: './hedef-secici.html',
  styleUrl: './hedef-secici.scss',
})
export class HedefSecici {
  readonly kullanicilar = input.required<Kullanici[]>();
  readonly tip = model<HedefTipi>('Hepsi');
  /** Departman adı (Departman) veya virgülle ayrılmış kullanıcı Id'leri (Kisi). */
  readonly deger = model<string>('');

  readonly ara = signal('');
  readonly aktifler = computed(() => this.kullanicilar().filter(k => k.aktif));
  readonly departmanlar = computed(() => [...new Set(this.aktifler().map(k => k.departman).filter((d): d is string => !!d))].sort());
  readonly secili = computed(() => new Set(this.deger().split(',').filter(Boolean).map(Number)));
  readonly filtreli = computed(() => {
    const a = this.ara().toLocaleLowerCase('tr');
    return this.aktifler().filter(k => !a || k.ad.toLocaleLowerCase('tr').includes(a) || (k.departman ?? '').toLocaleLowerCase('tr').includes(a));
  });
  readonly hedefSayisi = computed(() => {
    const t = this.tip();
    if (t === 'Hepsi') return this.aktifler().length;
    if (t === 'Departman') return this.aktifler().filter(k => k.departman === this.deger()).length;
    return this.secili().size;
  });

  tipSec(t: HedefTipi) {
    this.tip.set(t);
    this.deger.set(t === 'Departman' ? (this.departmanlar()[0] ?? '') : '');
  }

  kisiDegistir(id: number, secildi: boolean) {
    const s = new Set(this.secili());
    secildi ? s.add(id) : s.delete(id);
    this.deger.set([...s].join(','));
  }
  hepsiniSec() { this.deger.set(this.filtreli().map(k => k.id).join(',')); }
  temizle() { this.deger.set(''); }
}
