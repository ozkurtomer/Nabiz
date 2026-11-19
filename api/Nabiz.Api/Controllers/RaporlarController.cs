using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nabiz.Api.Security;
using Nabiz.Application.Dtos;
using Nabiz.Application.Services;

namespace Nabiz.Api.Controllers;

[ApiController, Route("api/raporlar"), Authorize(Roles = Roller.Rapor)]
public class RaporlarController(IRaporService raporlar, IDemoVeriService demo) : ControllerBase
{
    [HttpGet]
    public Task<RaporDto> Rapor([FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis, [FromQuery] string? departman, [FromQuery] int? paketId, CancellationToken ct) =>
        raporlar.GetirAsync(new RaporFiltre(baslangic, bitis, departman, paketId), ct);

    /// <summary>"Kim ne demiş": cevap/yorum listesi. İK/Admin isimli, Yönetici isimsiz ve yalnızca yorumlar.</summary>
    [HttpGet("cevaplar")]
    public Task<List<CevapSatiri>> Cevaplar([FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis, [FromQuery] string? departman, [FromQuery] int? paketId, [FromQuery] bool sadeceYorumlu, CancellationToken ct) =>
        raporlar.CevaplarAsync(new RaporFiltre(baslangic, bitis, departman, paketId), sadeceYorumlu, ct);

    [HttpGet("cevaplar/csv")]
    public async Task<FileContentResult> CevaplarCsv([FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis, [FromQuery] string? departman, [FromQuery] int? paketId, CancellationToken ct) =>
        File(await raporlar.CevaplarCsvAsync(new RaporFiltre(baslangic, bitis, departman, paketId), ct), "text/csv; charset=utf-8", $"nabiz-cevaplar-{DateTime.Now:yyyyMMdd}.csv");

    /// <summary>Bir çalışanın zaman içindeki skorları ve cevapları (yalnızca İK/Admin; denetim kaydına işlenir).</summary>
    [HttpGet("kisi/{kullaniciId:int}"), Authorize(Roles = Roller.AdminIK)]
    public Task<KisiGecmisiDto> KisiGecmisi(int kullaniciId, CancellationToken ct) => raporlar.KisiGecmisiAsync(kullaniciId, ct);

    /// <summary>Deneme: raporları görmek için sahte geçmiş veri üretir.</summary>
    [HttpPost("demo-veri"), Authorize(Roles = Roller.Admin)]
    public Task<DemoVeriSonucu> DemoUret(CancellationToken ct) => demo.UretAsync(ct);

    [HttpDelete("demo-veri"), Authorize(Roles = Roller.Admin)]
    public Task<DemoVeriSonucu> DemoTemizle(CancellationToken ct) => demo.TemizleAsync(ct);
}
