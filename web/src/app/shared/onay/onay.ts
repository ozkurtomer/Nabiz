import { Component, ElementRef, HostListener, effect, inject, viewChild } from '@angular/core';
import { OnayService } from '../../core/services/onay.service';

@Component({
  selector: 'app-onay',
  templateUrl: './onay.html',
  styleUrl: './onay.scss',
})
export class OnayPenceresi {
  readonly onay = inject(OnayService);
  private evetDugme = viewChild<ElementRef<HTMLButtonElement>>('evet');

  constructor() {
    // Pencere açılınca odak onay düğmesine gelsin (klavyeyle Enter/Esc çalışsın)
    effect(() => { if (this.onay.acik()) queueMicrotask(() => this.evetDugme()?.nativeElement.focus()); });
  }

  @HostListener('document:keydown.escape') esc() { if (this.onay.acik()) this.onay.kapat(false); }
}
