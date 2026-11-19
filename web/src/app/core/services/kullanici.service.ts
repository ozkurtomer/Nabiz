import { Injectable } from '@angular/core';
import { Kullanici, PersonelAktarimSonucu, Rol } from '../models/models';
import { ApiBase } from './api.base';

@Injectable({ providedIn: 'root' })
export class KullaniciService extends ApiBase {
  liste(ara?: string) { return this.http.get<Kullanici[]>(this.url('kullanicilar'), { params: ara ? { ara } : {} }); }
  guncelle(id: number, k: { rol: Rol; aktif: boolean; dil: string }) { return this.http.put<void>(this.url(`kullanicilar/${id}`), k); }
  personelAktar() { return this.http.post<PersonelAktarimSonucu>(this.url('kullanicilar/personel-aktar'), {}); }
}
