using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Application.Services;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/qr")]
public class QrApiController(ProductRepo products, QrService qrService, SettingRepo settings) : ControllerBase
{
    private static List<int> ParseIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return new List<int>();
        return ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var v) ? v : -1)
            .Where(v => v > 0)
            .Distinct()
            .ToList();
    }

    // GET /api/qr/print?ids=1,2,3 —— 打印标签数据
    [HttpGet("print")]
    public async Task<ActionResult<List<QrLabelDto>>> Print(string? ids)
    {
        var list = await products.GetByIdsAsync(ParseIds(ids));
        return Ok(list.Select(p => new QrLabelDto(p.Id, p.Model, p.Name)).ToList());
    }

    // GET /api/qr/zip?ids=1,2,3 —— 打包下载 PNG
    [HttpGet("zip")]
    public async Task<IActionResult> Zip(string? ids)
    {
        var list = await products.GetByIdsAsync(ParseIds(ids));
        if (list.Count == 0) return NotFound();
        var baseUrl = await ProductsApiController.QrBaseUrlAsync(settings);
        var bytes = qrService.CreateZip(list.Select(p => (p.Id, p.Model, p.Name, $"{baseUrl}/product/{p.Id}")));
        return File(bytes, "application/zip", $"产品二维码_{DateTime.Now:yyyyMMddHHmm}.zip");
    }
}
