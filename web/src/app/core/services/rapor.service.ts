import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CevapSatiri, DemoVeriSonucu, KisiGecmisi, Rapor, RaporFiltre } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class RaporService extends ApiBase {
  private params(f: RaporFiltre, ekstra: Record<string, string> = {}) {
    let p = new HttpParams().set('baslangic', f.baslangic).set('bitis', f.bitis);
    if (f.departman) p = p.set('departman', f.departman);
    if (f.paketId) p = p.set('paketId', f.paketId);
    for (const [k, v] of Object.entries(ekstra)) p = p.set(k, v);
    return p;
  }
  getir(f: RaporFiltre) { return this.http.get<Rapor>(this.url('raporlar'), { params: this.params(f) }); }
  cevaplar(f: RaporFiltre, sadeceYorumlu: boolean) {
    return this.http.get<CevapSatiri[]>(this.url('raporlar/cevaplar'), { params: this.params(f, { sadeceYorumlu: String(sadeceYorumlu) }) });
  }
  csv(f: RaporFiltre) { return this.http.get(this.url('raporlar/cevaplar/csv'), { params: this.params(f), responseType: 'blob' }); }
  kisiGecmisi(id: number) { return this.http.get<KisiGecmisi>(this.url(`raporlar/kisi/${id}`)); }
  demoUret() { return this.http.post<DemoVeriSonucu>(this.url('raporlar/demo-veri'), {}); }
  demoTemizle() { return this.http.delete<DemoVeriSonucu>(this.url('raporlar/demo-veri')); }
}
