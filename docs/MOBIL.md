# Mobil uygulama entegrasyonu (WebView)

Nabız web arayüzü mobil uygulamanın içinde **WebView** ile açılır. Kullanıcı mobil uygulamada zaten giriş yaptığı için tekrar giriş istemez.

## Akış

```
Mobil uygulama (giriş yapmış kullanıcı)
   │ 1) kendi backend'i  ──POST /api/auth/mobil-giris──▶  Nabız API
   │    X-Mobil-Anahtar: <paylaşılan gizli anahtar>
   │    { "email": "calisan@sirket.com" }
   │ 2) { "url": "https://nabiz.sirket.com/giris/dogrula?t=…", "token": "…" }
   ▼
WebView, url'yi açar → Nabız token'ı doğrular → oturum başlar → kullanıcı "Sorularım" sayfasına düşer
```

- Token **15 dakika** geçerlidir ve yalnızca giriş için kullanılabilir (API oturumu olarak çalışmaz).
- Anahtar **yalnızca sunucudan sunucuya** kullanılmalıdır; mobil uygulamanın içine gömülmemelidir.
- Özellik, `Mobil__Anahtar` tanımlı değilse **kapalıdır** (uç 401 döner).

## Kurulum

1. Rastgele bir anahtar üret: `openssl rand -base64 32`
2. Nabız API'sine ver: ortam değişkeni `Mobil__Anahtar` (ya da `appsettings.json` → `Mobil:Anahtar`)
3. Mobil uygulamanın backend'i aynı anahtarla `POST /api/auth/mobil-giris` çağırır, dönen `url`'yi WebView'da açar.

Örnek:

```bash
curl -X POST https://nabiz.sirket.com/api/auth/mobil-giris \
  -H "Content-Type: application/json" \
  -H "X-Mobil-Anahtar: $MOBIL_ANAHTAR" \
  -d '{"email":"calisan@sirket.com"}'
```

## E-postadaki cevap linkleri

Çalışana giden `…/cevap/<token>` linkleri giriş gerektirmez; mobil tarayıcıda ya da WebView'da doğrudan açılır.
