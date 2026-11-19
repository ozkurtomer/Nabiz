import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { I18n } from '../../core/services/i18n';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  readonly auth = inject(AuthService);
  readonly i18n = inject(I18n);
  readonly menuAcik = signal(false);
  readonly bas = computed(() => (this.auth.ad().trim().charAt(0) || '?').toUpperCase());
  readonly rolAdi = computed(() => ({ IK: 'İnsan Kaynakları', Calisan: 'Çalışan' } as Record<string, string>)[this.auth.rol() ?? ''] ?? '');

  constructor() {
    // Sayfa değişince mobil menüyü kapat
    inject(Router).events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => this.menuAcik.set(false));
  }
}
