# Yol haritası ve öneriler

Nabız şu an **tek şirket için çalışan bir çekirdek**: paketler, gönderim, cevaplama, raporlar, gizlilik, iki dil. Bunu bir **ürün** olarak satmak istersen, aşağıdaki başlıklar önem sırasına göre düzenlendi.

## A) Satmadan önce şart olanlar (güven ve uyum)
Çalışan verisi hassas olduğundan, bir kurumsal müşteri bunları sormadan imza atmaz.

| Başlık | Neden | Not |
|---|---|---|
| **KVKK / GDPR paketi** | Yasal zorunluluk; satış engeli numara 1 | Aydınlatma metni ve açık rıza, veri saklama süresi + otomatik silme, "verimi indir / sil" talebi, veri işleme sözleşmesi şablonu |
| **SSO** (Microsoft Entra ID, Google) | Şirketler şifre/giriş linki istemez, merkezi yetki ister | Giriş linki yedek kalabilir; kullanıcı çıkınca erişim otomatik kapanır |
| **Çok kiracılı (multi-tenant) yapı** | Her müşteri verisi birbirinden izole olmalı | Şirket başına şema/anahtar, alt alan adı (`musteri.nabiz.com`), kendi SMTP ve logosu |
| **Güvenlik sağlamlaştırma** | Müşteri güvenlik anketi gönderir | Bağımsız sızma testi, veri şifreleme (dinlenirken), hız sınırı, 2 adımlı doğrulama, denetim kaydı dışa aktarma, ISO 27001 hazırlığı |
| **Yedek, izleme, SLA** | "Çöktü, veri gitti" korkusu | Otomatik yedek testi, uyarı sistemi (hata/yavaşlık), durum sayfası |

## B) Müşterinin "bunu alırım" dedirten özellikler
| Başlık | Değer |
|---|---|
| **Teams / Slack botu** | Soru kanalda gelir, tek tıkla cevaplanır. Cevap oranını en çok bu artırır |
| **Mailde tek tıkla cevap** | Emoji doğrudan mailin içinde; giriş ve ek sayfa gerekmez |
| **İK sistemi entegrasyonları** | Logo, Mikro, SAP, Workday, BambooHR ile otomatik personel senkronu (kurulumu tek tıkla) |
| **Mobil (PWA + push)** | Vardiyalı/sahadaki çalışanlar (üretim, perakende, lojistik) için ana kanal |
| **Aksiyon takibi** | Uyarıdan görev aç, kişiye ata, süre koy, "kapatıldı" ile sonucu ölç. "Anket yaptık ama bir şey olmadı" şikâyetinin çözümü |
| **Zamanlanmış rapor maili** | Her pazartesi İK'ya PDF/özet; panele girmeden değer görürler |
| **Yönetici görünümü (isteğe bağlı)** | Şu an yalnızca İK/Çalışan var. Büyük şirketler yöneticiye kendi ekibinin isimsiz özetini ister (minimum grup büyüklüğü kuralıyla) |

## C) Fark yaratan analitik
- **eNPS** ve **ayrılma riski skoru** (düşüş eğilimi + katılım düşüşü + düşük uyarılar birlikte)
- **Isı haritası**: departman × hafta; sorunlu noktayı tek bakışta görme
- **Sektör kıyası (benchmark)**: "Sizin skorunuz sektör ortalamasının %8 altında". Müşteri sayısı büyüdükçe ürünün kendi veri avantajı olur (anonim, toplulaştırılmış)
- **Anomali uyarısı**: bir departmanda bir haftada ani düşüş olursa İK'ya otomatik bildirim
- **Yorum özetleme ve temalandırma**: yüzlerce serbest yorumu "ana konular" olarak çıkarma (ayrı ve **açıkça isteğe bağlı**, veri dışarı gittiği için müşteri onayıyla)
- **Önce/sonra etkisi**: bir aksiyon sonrası skor değişimi

## D) İçerik ve deneyim
- **Uzman onaylı soru kütüphanesi**: psikolog/İK danışmanı onaylı paketler ürünün en büyük farkı olabilir; hazır içerik müşteriyi hızla başlatır
- **Daha fazla dil** (Almanca, Arapça, Rusça…) ve sağdan sola yazım desteği
- **Teşekkür / takdir (peer recognition)** ve **anonim öneri kutusu**: anketin ötesine geçip günlük kullanım sağlar
- **Erişilebilirlik** (ekran okuyucu, klavye, kontrast): kamu ve büyük şirket ihalelerinde şart

## E) Ürünleştirme ve satış
| Konu | Öneri |
|---|---|
| **Fiyatlandırma** | Çalışan başına/ay (ör. 1-2 €), kademeli indirim, yıllık ödemede avantaj. Küçük şirket için ücretsiz katman (50 kişiye kadar) |
| **Pilot modeli** | 4 hafta, tek departman, ücretsiz. Sonunda "cevap oranı %78, 3 erken uyarı yakalandı" gibi somut rapor sun |
| **ROI hikâyesi** | Bir çalışanın ayrılma maliyeti (işe alım + eğitim + verim kaybı) genelde yıllık maaşının %50-200'ü. "Tek bir ayrılığı önlemek yıllık aboneliği öder" |
| **Hedef segment** | Çok şubeli/vardiyalı şirketler (perakende, üretim, çağrı merkezi, lojistik): hızlı büyür, İK ekipleri uzaktan görüş ister |
| **Kendi site ve demo** | Landing page, canlı demo ortamı (bu projedeki demo modu işe yarar), kısa tanıtım videosu |
| **Kurulum** | Müşteri için "personel tablosu + SMTP" ayarını sihirbazlaştırmak (şu an `.env` dosyası) |
| **Public API ve webhook'lar** | Müşteri kendi sistemine veri çeksin / olay alsın |
| **Rakipler** | Sorwe, Officevibe, Peakon (Workday), Culture Amp: yerel dil, KVKK uyumu, yerel destek ve fiyat avantajı ana kozlar |

## Önerilen 90 günlük sıra
1. **0-30 gün:** Gerçek SMTP + gerçek personel tablosuyla **tek departmanda pilot**, geri bildirim topla. KVKK metinleri ve SSO başla.
2. **30-60 gün:** Aksiyon takibi, zamanlanmış rapor maili, mailde tek tıkla cevap, Teams botu.
3. **60-90 gün:** Çok kiracılı yapı, fiyatlandırma/abonelik, landing page, ilk 2-3 ücretli müşteri.

> En kritik tek şey: **gerçek kullanımdan öğrenmek.** Pilotsuz ürünleştirmeye girişme; hangi özelliğin gerçekten istendiğini pilot gösterir.
