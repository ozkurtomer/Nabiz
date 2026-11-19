import { computed, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { LoginYanit, Rol } from '../models/models';
import { ApiBase } from './api.base';
import { I18n } from './i18n';

const ANAHTAR = 'nabiz.oturum';

@Injectable({ providedIn: 'root' })
export class AuthService extends ApiBase {
  private router = inject(Router);
  private i18n = inject(I18n);
  private oturum = signal<LoginYanit | null>(this.oku());

  readonly token = computed(() => this.oturum()?.token ?? null);
  readonly rol = computed<Rol | null>(() => this.oturum()?.rol ?? null);
  readonly ad = computed(() => this.oturum()?.ad ?? '');
  readonly girisYapti = computed(() => !!this.oturum());

  login(email: string, sifre: string) {
    return this.http.post<LoginYanit>(this.url('auth/login'), { email, sifre }).pipe(
      tap(r => { this.oturum.set(r); this.yaz(r); this.i18n.dil.set(r.dil); }));
  }

  /** Şifresiz giriş: e-postaya 15 dakikalık giriş linki gönderir. */
  girisLinkiIste(email: string) { return this.http.post<void>(this.url('auth/giris-linki'), { email }); }

  girisLinkiDogrula(token: string) {
    return this.http.post<LoginYanit>(this.url('auth/giris-dogrula'), { token }).pipe(
      tap(r => { this.oturum.set(r); this.yaz(r); this.i18n.dil.set(r.dil); }));
  }

  cikis() {
    this.oturum.set(null);
    try { localStorage.removeItem(ANAHTAR); } catch { /* yoksay */ }
    this.router.navigate(['/giris']);
  }

  rolVar(...roller: Rol[]) { const r = this.rol(); return !!r && roller.includes(r); }

  private oku(): LoginYanit | null {
    try { return JSON.parse(localStorage.getItem(ANAHTAR) ?? 'null'); } catch { return null; }
  }
  private yaz(r: LoginYanit) { try { localStorage.setItem(ANAHTAR, JSON.stringify(r)); } catch { /* yoksay */ } }
}
