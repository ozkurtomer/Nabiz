import { Injectable } from '@angular/core';
import { GonderimSonucu, Plan, PlanKaydet } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class PlanService extends ApiBase {
  liste() { return this.http.get<Plan[]>(this.url('planlar')); }
  ekle(p: PlanKaydet) { return this.http.post<Plan>(this.url('planlar'), p); }
  guncelle(id: number, p: PlanKaydet) { return this.http.put<Plan>(this.url(`planlar/${id}`), p); }
  sil(id: number) { return this.http.delete<void>(this.url(`planlar/${id}`)); }
  simdiGonder(id: number) { return this.http.post<GonderimSonucu>(this.url(`planlar/${id}/simdi-gonder`), {}); }
}
