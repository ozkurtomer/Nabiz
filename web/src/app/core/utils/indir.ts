/** Sunucudan gelen dosyayı (blob) tarayıcıda indirir. */
export function indir(blob: Blob, dosyaAdi: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url; a.download = dosyaAdi; a.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
