import { Injectable } from '@angular/core';
import { Denetim, SistemDurumu } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class SistemService extends ApiBase {
  durum() { return this.http.get<SistemDurumu>(this.url('sistem/durum')); }
  denetim() { return this.http.get<Denetim[]>(this.url('sistem/denetim')); }
}
