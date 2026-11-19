# Operasyon notları

## Ortam ayarları
Tüm ayarlar `api/Nabiz.Api/appsettings.json` ya da ortam değişkenleridir (`Bolum__Anahtar` biçimi, ör. `Smtp__Host`).

| Ayar | Varsayılan | Açıklama |
|---|---|---|
| `Hatirlatma:Aktif` | `true` | Cevaplamayanlara otomatik hatırlatma maili |
| `Hatirlatma:GunSonra` | `2` | Gönderimden / son hatırlatmadan kaç gün sonra |
| `Hatirlatma:Maks` | `2` | Kişi başına en fazla hatırlatma sayısı |
| `Bildirim:IkMailAktif` | `true` | Düşük puanda İK'ya e-posta |
| `Bildirim:WebhookUrl` | boş | Teams / Slack gelen webhook adresi; boşsa kapalı |
| `Mobil:Anahtar` | boş | WebView otomatik giriş anahtarı (bkz. [MOBIL.md](MOBIL.md)) |
| `Zamanlayici:KontrolSaniye` | `60` | Plan/hatırlatma kontrol aralığı |
| `Zamanlayici:PersonelSenkronDakika` | `60` | Personel tablosu senkron aralığı |
| `Ozet:Aktif` | `true` | İK'ya haftalık özet e-postası (SMTP ayarlıysa) |
| `Ozet:Gun` / `Ozet:Saat` | `1` / `9` | Gün (0=Pazar, 1=Pazartesi…) ve saatten itibaren haftada bir kez gönderilir |

Hatırlatmalar yalnızca **hafta içi 09:00-18:00** (şirket saat diliminde) gönderilir; demo ve test gönderimlerine hatırlatma gitmez.

## Diller
- Arayüz dili sağ alttaki **TR / EN** düğmesiyle değişir ve kullanıcının profiline kaydedilir; **e-postalar kullanıcının kayıtlı dilinde** gider.
- Sunucu metinleri (paket adı, soru metni, hata mesajı) istekteki `X-Dil` başlığına göre seçilir.
- Soru ve paketlerde Türkçe/İngilizce metinlerden **en az biri** zorunludur; seçili dilde metin yoksa diğer dil gösterilir.
- Yeni arayüz metni eklerken Türkçe kaynak metni `{{ 'metin' | t }}` ile sar ve İngilizcesini `web/src/app/core/i18n/en.ts` sözlüğüne ekle.

## Teams / Slack bildirimi
Teams'te "Workflows → Webhook isteği alındığında" (ya da Slack'te Incoming Webhook) ile bir adres oluşturup `Bildirim__WebhookUrl` olarak ver. Gönderilen mesajda **kişi bilgisi yoktur**, yalnızca "yeni düşük puan uyarısı" ve panel linki bulunur.

## Roller ve gizlilik
Yalnızca iki rol vardır:

| Rol | Yapabildiği |
|---|---|
| **Çalışan** | Mail linkiyle ya da "Sorularım" sayfasından kendisine gelen paketleri cevaplar. Başkasının verisini göremez. |
| **İK** | Raporlar, gönderimler (hemen / bir kez / tekrarlayan), uyarılar, paket ve soru yönetimi, kullanıcı rolleri, ayarlar, denetim kaydı, demo veri. |

- İK rolü, **Kullanıcılar** sayfasından bir çalışana verilir. İlk İK hesabı `Baslangic:IlkIK` ayarıyla açılışta oluşturulur.
- İK kişi bazlı veriyi görebilir; her erişim **Denetim kaydı** sayfasına işlenir.
- Çalışan yorumunu **anonim** bırakabilir: İK dahil kimse o yorumu kişiyle ilişkilendiremez.
- Cevap linkleri kişiye özel, tahmin edilemez token'lardır; paylaşılmamalıdır.

## Yedekleme
Uygulamanın tüm verisi `nabiz` şemasındadır (kişi/rol bilgisi personel tablosundan yeniden üretilir).

```sql
-- SQL Server tam yedek
BACKUP DATABASE [VERITABANI_ADI] TO DISK = N'/var/opt/mssql/backup/nabiz.bak' WITH COMPRESSION, CHECKSUM;
```

Docker'daki yerel deneme veritabanı için: `docker compose exec db /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "BACKUP DATABASE [Nabiz] TO DISK='/var/opt/mssql/backup/nabiz.bak'"`

## İzleme
- `GET /saglik` — basit canlılık kontrolü.
- **Ayarlar → Sistem durumu** — veritabanı, SMTP, son 24 saat mail sonuçları, son personel senkronu.
- Gitmeyen mailler **Gönderimler → ilgili gönderim → Tekrar dene** ile yeniden denenir.

## Migration (DBA için)
```bash
cd api && dotnet tool restore
dotnet ef migrations script --idempotent -p Nabiz.Infrastructure -s Nabiz.Api -o nabiz.sql
```

## Kurumsal özellikler

### eNPS ve ısı haritası
- *Puan* tipindeki bir soru **eNPS sorusu** olarak işaretlenebilir (*Sorular* sayfasında). Hazır paket: **eNPS (Çalışan Tavsiye Skoru)**.
- Hesap: 9-10 destekleyen, 7-8 pasif, 1-6 eleştiren; **skor = %destekleyen − %eleştiren** (−100…+100). Önceki dönemle farkı gösterilir.
- *Raporlar* sayfasında departman × hafta **ısı haritası** ve **Yazdır / PDF** düğmesi bulunur (tarayıcının yazdırma penceresinden PDF'e kaydedilir).

### Aksiyon takibi
Uyarılardan (**Aksiyon oluştur**) ya da *Aksiyonlar* sayfasından görev açılır: sorumlu (İK), öncelik, son tarih, durum (Açık / Devam ediyor / Tamamlandı / İptal), not. Son tarihi geçen açık aksiyonlar **Gecikti** olarak işaretlenir ve haftalık özette sayılır. Uyarıdan aksiyon açılınca uyarı otomatik **Takipte** olur. Tüm değişiklikler denetim kaydına işlenir.

### Gizlilik ve KVKK (*Gizlilik ve KVKK* sayfası)
- **Saklama süresi** (varsayılan 24 ay, 0 = süresiz): süresi dolan gönderim, cevap ve uyarılar her gün otomatik silinir; sayfada ne kadar veri sileceği önceden görünür.
- **Aydınlatma metni** (TR/EN): çalışan, cevaplamaya başlamadan önce görür; boşsa varsayılan metin kullanılır.
- **Kişi hakları** (*Kullanıcılar → kişi geçmişi → Veri hakları*): kişinin verisini **CSV olarak dışa aktar** (anonim yorumlar hariç) ya da **kalıcı sil** (cevap, uyarı, aksiyon). İkisi de denetim kaydına işlenir.
- **Şirket markası**: ad ve vurgu rengi giriş, cevaplama ekranları, panel ve e-postalarda kullanılır.

### Güvenlik
- Kimlik uçlarında (`/api/auth/*`) IP başına **dakikada 10**, genel API'de **dakikada 300** istek sınırı (aşılınca `429`).
- nginx `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` ve **Content-Security-Policy** başlıkları ekler. Farklı bir alan adından dış kaynak (ör. harici font) yükleyecekse `web/nginx.conf` içindeki CSP'yi genişlet.
- Ters proxy arkasında gerçek istemci IP'si `X-Forwarded-For` ile alınır; proxy'siz doğrudan internete açma.
