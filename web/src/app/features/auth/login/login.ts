import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { I18n } from '../../../core/services/i18n';
import { TPipe } from '../../../shared/t.pipe';
import { Ikon } from '../../../shared/ikon/ikon';

interface DemoHesap { etiket: string; ikon: string; email: string; sifre: string; }

@Component({
  selector: 'app-login',
  imports: [Ikon, FormsModule, TPipe],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  readonly i18n = inject(I18n);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  readonly demoGiris = environment.demoGiris;
  readonly demoHesaplar: DemoHesap[] = [
    { etiket: 'İK', ikon: 'shield', email: 'ik@nabiz.local', sifre: 'Demo123!' },
    { etiket: 'Çalışan', ikon: 'user', email: 'calisan@nabiz.local', sifre: 'Demo123!' },
  ];

  email = '';
  sifre = '';
  readonly sifreModu = signal(false);
  readonly sifreGorunur = signal(false);
  readonly hata = signal(false);
  readonly bekliyor = signal(false);
  readonly linkGitti = signal(false);

  gir() {
    if (!this.email.trim() || this.bekliyor()) return;
    this.hata.set(false);
    this.bekliyor.set(true);
    if (this.sifreModu()) {
      this.auth.login(this.email.trim(), this.sifre).subscribe({
        next: () => this.router.navigateByUrl(this.route.snapshot.queryParams['donus'] || '/'),
        error: () => { this.hata.set(true); this.bekliyor.set(false); },
      });
    } else {
      this.auth.girisLinkiIste(this.email.trim()).subscribe({
        next: () => { this.linkGitti.set(true); this.bekliyor.set(false); },
        error: () => this.bekliyor.set(false),
      });
    }
  }

  demoIleGir(h: DemoHesap) {
    this.email = h.email; this.sifre = h.sifre; this.sifreModu.set(true);
    this.gir();
  }

  yeniden() { this.linkGitti.set(false); }
}
