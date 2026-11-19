using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Nabiz.Application.Abstractions;
using Nabiz.Application.Dtos;
using Nabiz.Domain.Entities;
using Nabiz.Domain.Enums;

namespace Nabiz.Application.Services;

public interface IDemoVeriService
{
    /// <summary>Son 60 gün için mevcut kullanıcılara sahte (demo) gönderim ve cevaplar üretir.</summary>
    Task<DemoVeriSonucu> UretAsync(CancellationToken ct);
    /// <summary>Yalnızca demo gönderimlerini siler; gerçek verilere dokunmaz.</summary>
    Task<DemoVeriSonucu> TemizleAsync(CancellationToken ct);
}

public class DemoVeriService(IUnitOfWork uow, IZaman zaman, IAyarService ayarlar, IDenetimService denetim) : IDemoVeriService
{
    private static readonly string[] Olumsuz =
    [
        "Bu hafta iş yükü çok fazlaydı.", "Toplantılardan işime vakit kalmıyor.", "Kendimi biraz tükenmiş hissediyorum.",
        "Hedefler net değil, ne beklendiğini bilmiyorum.", "Uzun süredir izin kullanamadım.", "Ekipte iletişim kopuk.",
    ];
    private static readonly string[] Notr = ["Fena değil, idare ediyor.", "Normal bir hafta.", "Bazı günler iyi, bazı günler zor."];
    private static readonly string[] Olumlu =
    [
        "Ekip arkadaşlarım harika.", "Yöneticimden çok destek aldım.", "Bu hafta çok verimliydi.",
        "Yeni proje beni çok heyecanlandırıyor.", "Esnek çalışma düzeni çok iyi geliyor.",
    ];

    public async Task<DemoVeriSonucu> UretAsync(CancellationToken ct)
    {
        var kullanicilar = await uow.Kullanicilar.Query.Where(k => k.Aktif).ToListAsync(ct);
        var paketler = await uow.Paketler.Query.Include(p => p.Sorular).ThenInclude(s => s.Soru).Where(p => p.Aktif).ToListAsync(ct);
        if (kullanicilar.Count == 0 || paketler.Count == 0) return new DemoVeriSonucu(0, 0);

        var a = await ayarlar.GetirAsync(ct);
        var rnd = new Random();
        var bugun = zaman.Simdi.Date;
        // Departman ve kişi bazlı kalıcı "moral" eğilimi: raporlarda anlamlı farklar çıksın
        var egilim = kullanicilar.Select(k => k.Departman ?? "—").Distinct().ToDictionary(d => d, _ => rnd.NextDouble() * 1.6 - 0.8);
        var kisiFarki = kullanicilar.ToDictionary(k => k.Id, _ => rnd.NextDouble() * 0.8 - 0.4);

        var adet = 0;
        for (var gun = 59; gun >= 0; gun--)
        {
            var tarih = bugun.AddDays(-gun);
            if (tarih.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            var paket = paketler[rnd.Next(paketler.Count)];
            var sorular = paket.Sorular.OrderBy(s => s.Sira).Select(s => s.Soru!).ToList();
            var gonderimZamani = zaman.UtcCevir(tarih.AddHours(9));
            var parti = new GonderimPartisi { PaketId = paket.Id, HedefTipi = HedefTipi.Hepsi, Zaman = gonderimZamani, Demo = true };

            foreach (var k in kullanicilar)
            {
                var tamam = rnd.NextDouble() < 0.75;   // %25 cevap vermez
                var g = new Gonderim
                {
                    Parti = parti, KullaniciId = k.Id, GonderimZamani = gonderimZamani, Durum = GonderimDurumu.Gonderildi,
                    Token = "demo-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(20)).ToLowerInvariant(),
                    Tamamlandi = tamam, TamamlanmaZamani = tamam ? gonderimZamani.AddMinutes(rnd.Next(5, 400)) : null,
                };
                if (tamam)
                {
                    var ruhHali = Math.Clamp(0.62 + egilim[k.Departman ?? "—"] * 0.25 + kisiFarki[k.Id] * 0.25
                        + Math.Sin(gun / 4.0) * 0.06 + (rnd.NextDouble() - 0.5) * 0.45, 0, 1);
                    foreach (var soru in sorular)
                    {
                        var oynama = Math.Clamp(ruhHali + (rnd.NextDouble() - 0.5) * 0.25, 0, 1);
                        var deger = soru.Tip switch
                        {
                            SoruTipi.Emoji => 1 + (int)Math.Round(oynama * 4),
                            SoruTipi.Puan => 1 + (int)Math.Round(oynama * 9),
                            _ => oynama > 0.45 ? 1 : 0,
                        };
                        var skor = soru.Skor(deger);
                        var yorum = rnd.NextDouble() < (skor < 40 ? 0.6 : 0.2) ? Sec(skor < 40 ? Olumsuz : skor < 70 ? Notr : Olumlu, rnd) : null;
                        var c = new Cevap { SoruId = soru.Id, Deger = deger, Yorum = yorum, GuncellemeZamani = g.TamamlanmaZamani!.Value };
                        g.Cevaplar.Add(c);
                        if (soru.DusukMu(deger, a.EmojiEsik, a.PuanEsik))
                            uow.Uyarilar.Add(new Uyari { Cevap = c, Olusturma = c.GuncellemeZamani, Durum = gun > 5 ? UyariDurumu.Kapandi : UyariDurumu.Yeni });
                    }
                }
                uow.Gonderimler.Add(g);
                adet++;
            }
        }
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("Demo veri üretildi", $"{adet}", ct);
        return new DemoVeriSonucu(adet, 0);
    }

    public async Task<DemoVeriSonucu> TemizleAsync(CancellationToken ct)
    {
        var demo = await uow.GonderimPartileri.Query.Where(p => p.Demo).ToListAsync(ct);
        uow.GonderimPartileri.RemoveRange(demo);   // gönderimler, cevaplar ve uyarılar veritabanında cascade ile silinir
        await uow.SaveChangesAsync(ct);
        await denetim.KaydetAsync("Demo veri silindi", $"{demo.Count}", ct);
        return new DemoVeriSonucu(0, demo.Count);
    }

    private static string Sec(string[] liste, Random r) => liste[r.Next(liste.Length)];
}
