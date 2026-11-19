import { Component, computed, input } from '@angular/core';

/** Çizgi ikon seti (24x24, stroke). Kütüphane gerekmez; yalnızca path verisi içerir. */
const IKONLAR: Record<string, string> = {
  send: 'M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z',
  plus: 'M12 5v14M5 12h14',
  refresh: 'M21 12a9 9 0 1 1-3-6.7M21 4v5h-5',
  download: 'M12 3v12m0 0l-4-4m4 4l4-4M4 17v3h16v-3',
  trash: 'M4 7h16M10 11v6M14 11v6M6 7l1 13h10l1-13M9 7V4h6v3',
  edit: 'M4 20h4L19 9l-4-4L4 16v4zM13 7l4 4',
  search: 'M11 4a7 7 0 1 0 0 14 7 7 0 0 0 0-14zM21 21l-5-5',
  check: 'M5 12l5 5L20 7',
  'check-circle': 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM8 12l3 3 5-6',
  alert: 'M12 3l10 18H2L12 3zM12 10v5M12 18v.01',
  info: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM12 11v5M12 8v.01',
  'x-circle': 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM9 9l6 6M15 9l-6 6',
  mail: 'M3 6h18v12H3zM3 7l9 7 9-7',
  users: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 3a4 4 0 1 0 0 8 4 4 0 0 0 0-8zM22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8',
  user: 'M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2M12 3a4 4 0 1 0 0 8 4 4 0 0 0 0-8z',
  building: 'M4 21V5l8-2v18M12 9l8 2v10M2 21h20M8 9v.01M8 13v.01M8 17v.01M16 14v.01M16 18v.01',
  calendar: 'M5 4h14a2 2 0 0 1 2 2v13a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2zM16 2v4M8 2v4M3 10h18',
  clock: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM12 7v5l3 2',
  repeat: 'M17 2l4 4-4 4M3 11V9a3 3 0 0 1 3-3h15M7 22l-4-4 4-4M21 13v2a3 3 0 0 1-3 3H3',
  zap: 'M13 2L4 14h7l-1 8 9-12h-7l1-8z',
  chart: 'M3 3v18h18M7 15l4-4 4 3 5-7',
  bell: 'M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9M13.7 21a2 2 0 0 1-3.4 0',
  package: 'M21 8l-9-5-9 5 9 5 9-5zM3 8v8l9 5 9-5V8M12 13v8',
  help: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM9.1 9a3 3 0 0 1 5.8 1c0 2-3 3-3 3M12 17v.01',
  shield: 'M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6l8-3zM9 12l2 2 4-4',
  sliders: 'M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6',
  logout: 'M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9',
  inbox: 'M22 12h-6l-2 3h-4l-2-3H2M5.5 5.1L2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.5-6.9A2 2 0 0 0 16.8 4H7.2a2 2 0 0 0-1.7 1.1z',
  'eye-off': 'M3 3l18 18M10.6 6.1A10 10 0 0 1 12 6c6 0 10 6 10 6a17 17 0 0 1-3 3.6M6.6 6.6A17 17 0 0 0 2 12s4 6 10 6a9.7 9.7 0 0 0 4.2-1M9.9 9.9a3 3 0 0 0 4.2 4.2',
  file: 'M14 3H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9l-6-6zM14 3v6h6M8 13h8M8 17h5',
  globe: 'M12 3a9 9 0 1 0 0 18 9 9 0 0 0 0-18zM3 12h18M12 3c3 3 3 15 0 18M12 3c-3 3-3 15 0 18',
  play: 'M7 4l13 8-13 8V4z',
  lock: 'M6 11h12v10H6zM8 11V7a4 4 0 0 1 8 0v4',
  activity: 'M3 12h4l3-8 4 16 3-8h4',
  flask: 'M9 3h6M10 3v6L4 19a2 2 0 0 0 2 3h12a2 2 0 0 0 2-3l-6-10V3',
  'arrow-left': 'M19 12H5M12 19l-7-7 7-7',
  'arrow-right': 'M5 12h14M12 5l7 7-7 7',
  'chevron-down': 'M6 9l6 6 6-6',
  tasks: 'M9 6h11M9 12h11M9 18h11M3.5 6l1 1 2-2M3.5 12l1 1 2-2M3.5 18l1 1 2-2',
  flag: 'M5 21V4M5 4h11l-2 4 2 4H5',
  trend: 'M3 17l6-6 4 4 8-8M15 7h6v6',
  printer: 'M6 9V3h12v6M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2M6 14h12v7H6z',
  link: 'M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1',
  heart: 'M20.8 5.6a5.5 5.5 0 0 0-7.8 0L12 6.7l-1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-7.6a5.5 5.5 0 0 0 0-7.8z',
};

@Component({
  selector: 'app-ikon',
  template: `<svg class="ik" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path [attr.d]="yol()"/></svg>`,
  styles: [':host { display: inline-flex; line-height: 0; } svg { width: var(--ikon-boyut, 1.15em); height: var(--ikon-boyut, 1.15em); fill: none; stroke: currentColor; stroke-width: 1.8; stroke-linecap: round; stroke-linejoin: round; }'],
})
export class Ikon {
  readonly ad = input.required<string>();
  readonly yol = computed(() => IKONLAR[this.ad()] ?? IKONLAR['info']);
}
