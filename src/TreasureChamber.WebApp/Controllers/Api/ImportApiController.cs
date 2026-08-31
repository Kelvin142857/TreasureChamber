using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Application.Dtos;
using TreasureChamber.Application.Services;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/import")]
public class ImportApiController(ProductImportService importService, ProductRepo products, IWebHostEnvironment env) : ControllerBase
{
    private const string SessionTempFile = "ImportTempFile";
    private string TempDir => Path.Combine(env.ContentRootPath, "App_Data", "temp");

    // GET /api/import/template —— 下载导入模板
    [HttpGet("template")]
    public IActionResult Template()
    {
        var bytes = importService.CreateTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "产品导入模板.xlsx");
    }

    // POST /api/import/preview —— 上传并预览（不写库）
    [HttpPost("preview")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ImportPreviewDto>> Preview(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("请选择要导入的文件");
        if (!ProductImportService.IsSupportedFile(file.FileName)) return BadRequest("仅支持 .xlsx 或 .csv 文件");

        Directory.CreateDirectory(TempDir);
        var savedPath = Path.Combine(TempDir, $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");
        await using (var stream = new FileStream(savedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var preview = await importService.ParseAsync(savedPath, file.FileName);
        HttpContext.Session.SetString(SessionTempFile, savedPath);

        var truncated = preview.Rows.Count > 300;
        var rows = truncated ? preview.Rows.Take(300).ToList() : preview.Rows;
        return Ok(new ImportPreviewDto(
            preview.FileName,
            rows.Select(r => new ImportRowDto(
                r.RowNumber, r.Model, r.Name, r.Series, r.Category,
                r.Action switch
                {
                    ImportAction.New => "New",
                    ImportAction.Update => "Update",
                    _ => "Error"
                }, r.Error)).ToList(),
            preview.NewCount, preview.UpdateCount, preview.ErrorCount, truncated));
    }

    // POST /api/import/commit —— 确认入库
    [HttpPost("commit")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ImportResultDto>> Commit()
    {
        var savedPath = HttpContext.Session.GetString(SessionTempFile);
        if (string.IsNullOrEmpty(savedPath) || !System.IO.File.Exists(savedPath))
            return BadRequest("预览文件已失效，请重新上传");

        var result = await importService.CommitAsync(savedPath);
        System.IO.File.Delete(savedPath);
        HttpContext.Session.Remove(SessionTempFile);
        return Ok(new ImportResultDto(result.Added, result.Updated, result.Skipped, result.Errors));
    }

    // POST /api/import/images —— 文件名匹配型号批量上传
    [HttpPost("images")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ImportImagesResultDto>> UploadImages(IList<IFormFile> files)
    {
        var matched = new List<ImportImagesItemDto>();
        var unmatched = new List<string>();
        var uploaded = 0;

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".gif")) continue;
            var baseName = Path.GetFileNameWithoutExtension(file.FileName).Trim();
            var model = baseName.Split('-', 2)[0].Trim();
            if (model.Length == 0) { unmatched.Add(file.FileName); continue; }

            var product = await products.GetByModelAsync(model);
            if (product == null) { unmatched.Add(file.FileName); continue; }

            var uploadDir = Path.Combine(env.WebRootPath, "uploads", product.Id.ToString());
            Directory.CreateDirectory(uploadDir);
            var maxOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder);
            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, safeName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            product.Images.Add(new Core.Entities.ProductImage { Path = $"uploads/{product.Id}/{safeName}", SortOrder = ++maxOrder });
            await products.SaveAsync(product);
            matched.Add(new ImportImagesItemDto(model, file.FileName));
            uploaded++;
        }

        return Ok(new ImportImagesResultDto(uploaded, matched, unmatched));
    }
}
