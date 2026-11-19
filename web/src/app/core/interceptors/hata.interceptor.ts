import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotifyService } from '../services/notify.service';

/** Oturum düşmesini yönetir, sunucu hata mesajlarını kullanıcıya gösterir. */
export const hataInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const notify = inject(NotifyService);
  return next(req).pipe(catchError((e: HttpErrorResponse) => {
    const girisIstegi = req.url.includes('/auth/');
    if (e.status === 401 && !girisIstegi) { auth.cikis(); }
    else if (!girisIstegi) {
      notify.hata(e.status === 0 ? 'Sunucuya ulaşılamıyor.' : (e.error?.detail ?? 'Bir hata oluştu.'));
    }
    return throwError(() => e);
  }));
};
