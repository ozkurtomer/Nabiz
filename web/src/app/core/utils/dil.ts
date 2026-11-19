/** Çok dilli metin seçimi: istenen dilde yazılmadıysa diğer dile düşer (iki dilden yalnızca biri zorunludur). */
export function sec(dil: 'tr' | 'en', tr?: string | null, en?: string | null): string {
  const asil = dil === 'en' ? en : tr;
  const yedek = dil === 'en' ? tr : en;
  return (asil && asil.trim()) ? asil : (yedek ?? '');
}
