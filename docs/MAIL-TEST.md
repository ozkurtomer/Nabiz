# Mail gerçekten geliyor mu? Nasıl test ederim?

## Önce şunu bil
Docker'ı varsayılan haliyle çalıştırınca mailler **gerçek kutulara gitmez**; **Mailpit** adlı sahte bir SMTP sunucusu yakalar ve http://localhost:8025 adresinde gösterir. Yani orada mail görmek, uygulamanın mail *ürettiğini* kanıtlar ama gerçek bir gelen kutusuna ulaştığını kanıtlamaz.
Gerçek teslimatı görmek için uygulamayı gerçek bir SMTP sunucusuna bağlayıp **kendi adresine** test maili göndermen gerekir.

## Adım adım (Gmail ile, 5 dakika)
1. Google Hesabı → Güvenlik → **2 Adımlı Doğrulama**'yı aç.
2. Aynı sayfada **Uygulama şifreleri**'nden bir şifre üret (16 haneli).
3. Proje klasöründe:
   ```bash
   cp .env.ornek .env
   ```
   `.env` içindeki `SMTP_USER`, `SMTP_PASS` ve `SMTP_FROM` değerlerini kendi Gmail adresin ve uygulama şifrenle doldur.
4. Yeniden başlat:
   ```bash
   docker compose up -d
   ```
5. http://localhost:4200 → **İK** demo butonuyla gir → **Ayarlar → E-posta testi**.
6. "Test adresi" kutusuna kendi e-postanı yaz → **Test maili gönder**.
   - ✅ **Gönderildi** yazarsa gelen kutunu (ve **spam** klasörünü) kontrol et; birkaç saniye içinde mail gelir.
   - ❌ **Gönderilemedi** yazarsa nedeni ekranda görünür (yanlış şifre, 2 adımlı doğrulama kapalı, ağ engeli…).
7. **Ayarlar → Sistem durumu**'nda "SMTP (mail)" kutusu **Ayarlı**, "Son 24 saat mail" doğru sayıyı gösterir.

> Bu test, şirket SMTP'si ya da Microsoft 365 için de aynıdır; sadece `.env` değerleri değişir (örnekler `.env.ornek` içinde).

## Tüm akışı gerçek mailde görmek (soru maili + giriş linki)
Demo kullanıcıların adresleri sahte (`*.nabiz.local`) olduğu için gerçek SMTP'ye bağlıyken onlara mail gitmez. Kendi adresini bir demo çalışana vererek tüm akışı gerçek kutunda görebilirsin:

```bash
docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'Nabiz_Dev123!' -d Nabiz \
  -Q "UPDATE dbo.Personel SET Email='senin.adresin@gmail.com' WHERE Sicil='1003'"
```

Sonra **Kullanıcılar → Şimdi senkronize et** (Elif Çalışan'ın adresi güncellenir). Artık:
- **Gönderimler → Yeni gönderim → Kişi seç → Elif Çalışan** ile paket gönderirsen soru maili gerçek kutuna gelir; **Hemen cevapla** linkine tıklayarak cevaplarsın.
- Giriş ekranında "calisan" için **e-posta adresini yazıp "Giriş linki gönder"** dersen giriş linki gelir.

**Dikkat:** gerçek SMTP'ye bağlıyken **Herkese** gönderme; diğer demo adresler sahte olduğu için "mail hatası" görürsün (zararsız ama gürültülü).

## Mailpit'e geri dönmek
`.env` dosyasını sil (ya da yeniden adlandır) ve `docker compose up -d` çalıştır.

## Gerçek (şirket) ortamında
`.env.gercek` içindeki `Smtp__*` ayarlarını doldur ([README](../README.md#️-gerçek-ortama-alma)), aynı **E-posta testi** düğmesiyle doğrula.
