import { Injectable } from '@angular/core';
import { Soru, SoruKaydet } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class SoruService extends ApiBase {
  liste() { return this.http.get<Soru[]>(this.url('sorular')); }
  ekle(s: SoruKaydet) { return this.http.post<Soru>(this.url('sorular'), s); }
  guncelle(id: number, s: SoruKaydet) { return this.http.put<Soru>(this.url(`sorular/${id}`), s); }
}
