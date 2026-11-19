import { Injectable } from '@angular/core';
import { Uyari, UyariDurumu } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class UyariService extends ApiBase {
  liste() { return this.http.get<Uyari[]>(this.url('uyarilar')); }
  guncelle(id: number, durum: UyariDurumu, ikNotu?: string) { return this.http.put<void>(this.url(`uyarilar/${id}`), { durum, ikNotu }); }
}
