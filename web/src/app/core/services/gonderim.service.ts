import { Injectable } from '@angular/core';
import { AnlikGonder, GonderimSonucu, Parti, PartiDetay } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class GonderimService extends ApiBase {
  anlik(g: AnlikGonder) { return this.http.post<{ partiId: number; sonuc: GonderimSonucu }>(this.url('gonderimler/anlik'), g); }
  liste() { return this.http.get<Parti[]>(this.url('gonderimler')); }
  detay(id: number) { return this.http.get<PartiDetay>(this.url(`gonderimler/${id}`)); }
  csv(id: number) { return this.http.get(this.url(`gonderimler/${id}/csv`), { responseType: 'blob' }); }
  tekrarDene(id: number) { return this.http.post<GonderimSonucu>(this.url(`gonderimler/${id}/tekrar-dene`), {}); }
}
