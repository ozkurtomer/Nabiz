import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { I18n } from '../../core/services/i18n';
import { KullaniciService } from '../../core/services/kullanici.service';
import { TPipe } from '../../shared/t.pipe';

@Component({
  selector: 'app-shell',
  imports: [TPipe, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18n);
  private kullanicilar = inject(KullaniciService);
  readonly menuAcik = signal(false);
  readonly bas = computed(() => (this.auth.ad().trim().charAt(0) || '?').toUpperCase());
  readonly rolAdi = computed(() => ({ IK: this.i18n.t('İnsan Kaynakları'), Calisan: this.i18n.t('Çalışan') } as Record<string, string>)[this.auth.rol() ?? ''] ?? '');

  constructor() {
    // Sayfa değişince mobil menüyü kapat
    inject(Router).events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => this.menuAcik.set(false));
  }

  /** Dili değiştirir ve kaydeder (sonraki e-postalar bu dilde gelir); sunucudan gelen metinler yenilensin diye sayfa yeniden yüklenir. */
  dilSec(d: 'tr' | 'en') {
    if (this.i18n.dil() === d) return;
    this.i18n.degistir(d);
    const yenile = () => location.reload();
    this.kullanicilar.dilKaydet(d).subscribe({ next: yenile, error: yenile });
  }
}
