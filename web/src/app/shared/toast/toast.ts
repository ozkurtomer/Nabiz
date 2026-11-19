import { Component, inject } from '@angular/core';
import { NotifyService } from '../../core/services/notify.service';

@Component({
  selector: 'app-toast',
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
})
export class Toast {
  readonly notify = inject(NotifyService);
}
