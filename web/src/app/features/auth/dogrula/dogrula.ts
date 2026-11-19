import { Component, inject, input, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TPipe } from '../../../shared/t.pipe';
import { Ikon } from '../../../shared/ikon/ikon';

/** E-postadaki giriş linkinin açtığı sayfa: token'ı doğrular ve oturumu başlatır. */
@Component({
  selector: 'app-dogrula',
  imports: [Ikon, RouterLink, TPipe],
  template: `
    <main class="kart dar" style="text-align:center;margin-top:80px">
      @if (hata()) {
        <div style="width:56px;height:56px;margin:0 auto 12px;border-radius:50%;background:var(--vurgu-soft);color:var(--vurgu);display:grid;place-items:center;--ikon-boyut:26px"><app-ikon ad="x-circle" /></div>
        <h2>{{ 'Giriş linki geçersiz' | t }}</h2>
        <p style="color:var(--soluk)">{{ 'Linkin süresi dolmuş ya da daha önce kullanılmış olabilir.' | t }}</p>
        <a routerLink="/giris"><button class="birincil">{{ 'Yeni link iste' | t }}</button></a>
      } @else {
        <div style="width:56px;height:56px;margin:0 auto 12px;border-radius:50%;background:var(--vurgu-soft);color:var(--vurgu);display:grid;place-items:center;--ikon-boyut:26px"><app-ikon ad="clock" /></div>
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
