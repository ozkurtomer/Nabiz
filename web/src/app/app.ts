import { Component, inject } from '@angular/core';
import { MarkaService } from './core/services/marka.service';
import { RouterOutlet } from '@angular/router';
import { OnayPenceresi } from './shared/onay/onay';
import { Toast } from './shared/toast/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toast, OnayPenceresi],
  template: `<router-outlet /><app-toast /><app-onay />`,
})
export class App { constructor() { inject(MarkaService).yukle(); } }
