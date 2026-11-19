import { Injectable } from '@angular/core';
import { Paket, PaketKaydet } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class PaketService extends ApiBase {
  liste() { return this.http.get<Paket[]>(this.url('paketler')); }
  ekle(p: PaketKaydet) { return this.http.post<Paket>(this.url('paketler'), p); }
  guncelle(id: number, p: PaketKaydet) { return this.http.put<Paket>(this.url(`paketler/${id}`), p); }
  sil(id: number) { return this.http.delete<void>(this.url(`paketler/${id}`)); }
}
