import { Injectable } from '@angular/core';
import { BenimSoru, CevapGirdisi, CevapKaydetYanit, CevapSayfasi } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class CevapService extends ApiBase {
  /** Token'lı link: giriş gerekmez. */
  getir(token: string) { return this.http.get<CevapSayfasi>(this.url(`cevap/${token}`)); }
  kaydet(token: string, cevaplar: CevapGirdisi[], tamamla: boolean) {
    return this.http.post<CevapKaydetYanit>(this.url(`cevap/${token}`), { cevaplar, tamamla });
  }
  benim() { return this.http.get<BenimSoru[]>(this.url('cevap/benim')); }
  testGonderimi(paketId: number) { return this.http.post<{ token: string }>(this.url(`cevap/test/${paketId}`), {}); }
}
