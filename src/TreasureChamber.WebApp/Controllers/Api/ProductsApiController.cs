using Microsoft.AspNetCore.Mvc;
using TreasureChamber.Application.Services;
using TreasureChamber.Core.Entities;
using TreasureChamber.Core.Models;
using TreasureChamber.Data.Repo;
using TreasureChamber.WebApp.Models;

namespace TreasureChamber.WebApp.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController(ProductRepo products, QrService qrService) : ControllerBase
{
    // GET /api/products?seriesId=&categoryId=&keyword=&page=&pageSize=&includeInactive=
    [HttpGet]
    public async Task<ActionResult<PagedDto<ProductCardDto>>> List(
        int? seriesId, int? categoryId, string? keyword, int page = 1, int pageSize = 12, bool includeInactive = false)
    {
        var result = await products.QueryAsync(new ProductQuery
        {
            SeriesId = seriesId,
            CategoryId = categoryId,
            Keyword = keyword,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        }, includeInactive);
        return Ok(new PagedDto<ProductCardDto>(
            result.Items.Select(ApiMapper.ToCard).ToList(),
            result.Total, result.Page, result.PageSize, result.TotalPages));
    }

    // GET /api/products/picker?keyword=
    [HttpGet("picker")]
    public async Task<ActionResult<List<ProductPickerDto>>> Picker(string? keyword)
    {
        var items = await products.SearchForPickerAsync(string.IsNullOrWhiteSpace(keyword) ? "" : keyword);
        return Ok(items.Select(p => new ProductPickerDto(p.Id, p.Model, p.Name)).ToList());
    }

    // GET /api/products/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Detail(int id)
    {
        var product = await products.GetDetailAsync(id);
        if (product == null) return NotFound();
        return Ok(ApiMapper.ToDetail(product));
    }

    // GET /api/products/{id}/qr —— 单产品二维码 PNG
    [HttpGet("{id:int}/qr")]
    public async Task<IActionResult> Qr(int id, [FromServices] SettingRepo settings)
    {
        var product = await products.GetByIdAsync(id);
        if (product == null) return NotFound();
        var baseUrl = await QrBaseUrlAsync(settings);
        var png = qrService.CreatePng($"{baseUrl}/product/{id}");
        return File(png, "image/png");
    }

    internal static async Task<string> QrBaseUrlAsync(SettingRepo settings)
    {
        var saved = await settings.GetAsync(SettingRepo.QrBaseUrl);
        return string.IsNullOrWhiteSpace(saved) ? "" : saved.Trim().TrimEnd('/');
    }

    // POST /api/products —— 新建
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProductDetailDto>> Create(ProductEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Model)) return BadRequest("型号不能为空");
        var model = dto.Model.Trim();
        var name = string.IsNullOrWhiteSpace(dto.Name) ? model : dto.Name.Trim();
        if (await products.GetByModelAsync(model) != null)
            return BadRequest($"型号 {model} 已存在");

        var product = new Product
        {
            Model = model,
            Name = name,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            SeriesId = dto.SeriesId,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.Now
        };
        await products.AddAsync(product);
        ApplySpecs(product, dto.SpecNames, dto.SpecValues);
        await products.SaveAsync(product);
        return Ok(ApiMapper.ToDetail((await products.GetDetailAsync(product.Id))!));
    }

    // PUT /api/products/{id} —— 更新
    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, ProductEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Model)) return BadRequest("型号不能为空");
        var product = await products.GetForEditAsync(id);
        if (product == null) return NotFound();
        var model = dto.Model.Trim();
        var dup = await products.GetByModelAsync(model);
        if (dup != null && dup.Id != id) return BadRequest($"型号 {model} 已存在");

        product.Model = model;
        product.Name = string.IsNullOrWhiteSpace(dto.Name) ? model : dto.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        product.SeriesId = dto.SeriesId;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.Now;

        product.Specs.Clear();
        ApplySpecs(product, dto.SpecNames, dto.SpecValues);
        await products.SaveAsync(product);
        return Ok(ApiMapper.ToDetail((await products.GetDetailAsync(id))!));
    }

    private static void ApplySpecs(Product product, List<string>? names, List<string>? values)
    {
        var order = 0;
        if (names == null) return;
        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i]?.Trim();
            var value = values != null && i < values.Count ? values[i]?.Trim() : null;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) continue;
            product.Specs.Add(new ProductSpec { Name = name, Value = value, SortOrder = ++order });
        }
    }

    // DELETE /api/products/{id}
    [HttpDelete("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, [FromServices] IWebHostEnvironment env)
    {
        var product = await products.GetForEditAsync(id);
        if (product == null) return NotFound();
        await products.RemoveProductAsync(product, env.WebRootPath);
        return NoContent();
    }

    // POST /api/products/{id}/images —— 上传图片（multipart files）
    [HttpPost("{id:int}/images")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImages(int id, IList<IFormFile> files, [FromServices] IWebHostEnvironment env)
    {
        var product = await products.GetForEditAsync(id);
        if (product == null) return NotFound();

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", id.ToString());
        Directory.CreateDirectory(uploadDir);
        var maxOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder);
        var uploaded = 0;

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp" or ".gif")) continue;
            var safeName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, safeName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            product.Images.Add(new ProductImage { Path = $"uploads/{id}/{safeName}", SortOrder = ++maxOrder });
            uploaded++;
        }
        await products.SaveAsync(product);
        return Ok(new { uploaded });
    }

    // DELETE /api/products/{id}/images/{imageId}
    [HttpDelete("{id:int}/images/{imageId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId, [FromServices] IWebHostEnvironment env)
    {
        var product = await products.GetForEditAsync(id);
        var image = product?.Images.FirstOrDefault(i => i.Id == imageId);
        if (product != null && image != null)
        {
            var file = Path.Combine(env.WebRootPath, image.Path.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
            product.Images.Remove(image);
            await products.SaveAsync(product);
        }
        return NoContent();
    }
}
