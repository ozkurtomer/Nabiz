import { Component, inject, input, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TPipe } from '../../../shared/t.pipe';

/** E-postadaki giriş linkinin açtığı sayfa: token'ı doğrular ve oturumu başlatır. */
@Component({
  selector: 'app-dogrula',
  imports: [RouterLink, TPipe],
  template: `
    <main class="kart dar" style="text-align:center;margin-top:80px">
      @if (hata()) {
        <div style="font-size:3rem">⛔</div>
        <h2>{{ 'Giriş linki geçersiz' | t }}</h2>
        <p style="color:var(--soluk)">{{ 'Linkin süresi dolmuş ya da daha önce kullanılmış olabilir.' | t }}</p>
        <a routerLink="/giris"><button class="birincil">{{ 'Yeni link iste' | t }}</button></a>
      } @else {
        <div style="font-size:3rem">⏳</div>
        <h2>{{ 'Giriş yapılıyor…' | t }}</h2>
      }
    </main>`,
})
export class Dogrula implements OnInit {
  /** ?t=... sorgu parametresi (withComponentInputBinding). */
  readonly t = input<string>();
  private auth = inject(AuthService);
  private router = inject(Router);
  readonly hata = signal(false);

  ngOnInit() {
    const token = this.t();
    if (!token) { this.hata.set(true); return; }
    this.auth.girisLinkiDogrula(token).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.hata.set(true),
    });
  }
}
