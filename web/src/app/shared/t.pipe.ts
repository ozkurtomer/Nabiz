import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18n } from '../core/services/i18n';

/** {{ 'Türkçe metin' | t }} → seçili dile çevirir. Dil değişince hemen güncellenmesi için "pure: false". */
@Pipe({ name: 't', pure: false })
export class TPipe implements PipeTransform {
  private i18n = inject(I18n);
  transform(metin: string | null | undefined, ...degerler: unknown[]): string {
    if (!metin) return '';
    return degerler.length ? this.i18n.f(metin, ...degerler) : this.i18n.t(metin);
  }
}
