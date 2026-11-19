import { Injectable } from '@angular/core';
import { Aksiyon, AksiyonDurumu, AksiyonKaydet, AksiyonOzet } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class AksiyonService extends ApiBase {
  liste(durum?: AksiyonDurumu) { return this.http.get<Aksiyon[]>(this.url('aksiyonlar'), { params: durum ? { durum } : {} }); }
  ozet() { return this.http.get<AksiyonOzet>(this.url('aksiyonlar/ozet')); }
  ekle(a: AksiyonKaydet) { return this.http.post<Aksiyon>(this.url('aksiyonlar'), a); }
  guncelle(id: number, a: AksiyonKaydet) { return this.http.put<Aksiyon>(this.url(`aksiyonlar/${id}`), a); }
  sil(id: number) { return this.http.delete<void>(this.url(`aksiyonlar/${id}`)); }
}
