import { Routes } from '@angular/router';
import { anasayfaYonlendir, girisGerekli, rolGerekli } from './core/guards/guards';

export const routes: Routes = [
  { path: 'giris', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
  { path: 'giris/dogrula', loadComponent: () => import('./features/auth/dogrula/dogrula').then(m => m.Dogrula) },
  // E-postadaki cevap linki: giriş gerekmez, kişiye özel gizli token yeterlidir.
  { path: 'cevap/:token', loadComponent: () => import('./features/survey/cevap/cevap').then(m => m.Cevap) },
  {
    path: '', canActivate: [girisGerekli],
    loadComponent: () => import('./layout/shell/shell').then(m => m.Shell),
    children: [
      { path: 'sorularim', loadComponent: () => import('./features/sorularim/sorularim').then(m => m.Sorularim) },
      { path: 'raporlar', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/raporlar/raporlar').then(m => m.Raporlar) },
      { path: 'gonderimler', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/gonderimler/gonderimler').then(m => m.Gonderimler) },
      { path: 'gonderimler/:id', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/gonderimler/detay/detay').then(m => m.GonderimDetay) },
      { path: 'uyarilar', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/uyarilar/uyarilar').then(m => m.Uyarilar) },
      { path: 'paketler', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/paketler/paketler').then(m => m.Paketler) },
      { path: 'kisi/:id', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/kisi/kisi').then(m => m.Kisi) },
      { path: 'denetim', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/denetim/denetim').then(m => m.DenetimSayfasi) },
      { path: 'kullanicilar', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/admin/kullanicilar/kullanicilar').then(m => m.Kullanicilar) },
      { path: 'sorular', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/admin/sorular/sorular').then(m => m.Sorular) },
      { path: 'planlar', redirectTo: 'gonderimler' },
      { path: 'ayarlar', canActivate: [rolGerekli('IK')], loadComponent: () => import('./features/admin/ayarlar/ayarlar').then(m => m.Ayarlar) },
      { path: '', pathMatch: 'full', canActivate: [anasayfaYonlendir], children: [] },
    ],
  },
  { path: '**', redirectTo: '' },
];
