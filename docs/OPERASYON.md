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

Hatırlatmalar yalnızca **hafta içi 09:00-18:00** (şirket saat diliminde) gönderilir; demo ve test gönderimlerine hatırlatma gitmez.

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
