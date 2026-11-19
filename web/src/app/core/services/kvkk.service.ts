import { Injectable } from '@angular/core';
import { KisiSilSonuc, KvkkAyar, SaklamaOnizleme, SaklamaSonucu } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class KvkkService extends ApiBase {
  getir() { return this.http.get<KvkkAyar>(this.url('kvkk')); }
  kaydet(a: KvkkAyar) { return this.http.put<void>(this.url('kvkk'), a); }
  onizle() { return this.http.get<SaklamaOnizleme>(this.url('kvkk/saklama')); }
  uygula() { return this.http.post<SaklamaSonucu>(this.url('kvkk/saklama'), {}); }
  disaAktar(id: number) { return this.http.get(this.url(`kvkk/kisi/${id}/disa-aktar`), { responseType: 'blob' }); }
  kisiSil(id: number) { return this.http.delete<KisiSilSonuc>(this.url(`kvkk/kisi/${id}`)); }
}
