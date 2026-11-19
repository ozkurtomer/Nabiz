import { Injectable } from '@angular/core';
import { Ayarlar } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class AyarService extends ApiBase {
  getir() { return this.http.get<Ayarlar>(this.url('ayarlar')); }
  kaydet(a: Ayarlar) { return this.http.put<void>(this.url('ayarlar'), a); }
}
