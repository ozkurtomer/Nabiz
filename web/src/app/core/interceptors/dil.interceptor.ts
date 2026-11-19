import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { I18n } from '../services/i18n';

/** Her API isteğine seçili arayüz dilini ekler; sunucu metinleri (paket adı, hata mesajı…) bu dile göre döner. */
export const dilInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ setHeaders: { 'X-Dil': inject(I18n).dil() } }));
