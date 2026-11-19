import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Rol } from '../models/models';
import { AuthService } from '../services/auth.service';

export const girisGerekli: CanActivateFn = (_r, state) => {
  const auth = inject(AuthService);
  return auth.girisYapti() || inject(Router).createUrlTree(['/giris'], { queryParams: { donus: state.url } });
};

export const rolGerekli = (...roller: Rol[]): CanActivateFn => () =>
  inject(AuthService).rolVar(...roller) || inject(Router).createUrlTree(['/']);

/** Açılış sayfası role göre: çalışan → Sorularım, İK → Raporlar. */
export const anasayfaYonlendir: CanActivateFn = () => {
  const auth = inject(AuthService);
  return inject(Router).createUrlTree([auth.rolVar('IK') ? '/raporlar' : '/sorularim']);
};
