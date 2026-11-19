import { SoruTipi } from '../models/models';

export const EMOJILER = ['😞', '🙁', '😐', '🙂', '😄'];

/** Cevap değerini okunur biçime çevirir (emoji / x/10 / Evet-Hayır). */
export function cevapGoster(tip: SoruTipi, deger: number): string {
  if (tip === 'Emoji') return EMOJILER[deger - 1] ?? String(deger);
  if (tip === 'Puan') return `${deger}/10`;
  return deger === 1 ? 'Evet' : 'Hayır';
}

/** 0-100 skoru renk sınıfına çevirir (<40 kötü, <70 orta, ≥70 iyi). */
export function skorSinifi(skor: number | null | undefined): 'kotu' | 'orta' | 'iyi' | 'yok' {
  if (skor === null || skor === undefined) return 'yok';
  return skor < 40 ? 'kotu' : skor < 70 ? 'orta' : 'iyi';
}
