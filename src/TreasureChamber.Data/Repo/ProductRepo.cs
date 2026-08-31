using Microsoft.EntityFrameworkCore;
using TreasureChamber.Core.Entities;
using TreasureChamber.Core.Models;

namespace TreasureChamber.Data.Repo;

public class ProductRepo(AppDbContext db) : BaseRepo(db)
{
    public async Task<PagedResult<Product>> QueryAsync(ProductQuery query, bool includeInactive = false)
    {
        var categoryIds = query.CategoryId == null
            ? null
            : (await Db.Categories.AsNoTracking().ToListAsync()).DescendantIdsIncludingSelf(query.CategoryId.Value);

        var q = Db.Products.AsNoTracking()
            .Include(p => p.Series)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        if (!includeInactive) q = q.Where(p => p.IsActive);
        if (query.SeriesId is int seriesId) q = q.Where(p => p.SeriesId == seriesId);
        if (categoryIds != null) q = q.Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            q = q.Where(p => p.Model.Contains(kw) || p.Name.Contains(kw));
        }

        var total = await q.CountAsync();
        var page = Math.Max(1, query.Page);
        var items = await q
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new Product
            {
                Id = p.Id,
                Model = p.Model,
                Name = p.Name,
                Description = p.Description,
                SeriesId = p.SeriesId,
                Series = p.Series,
                CategoryId = p.CategoryId,
                Category = p.Category,
                IsActive = p.IsActive,
                Images = p.Images.OrderBy(i => i.SortOrder).ToList()
            })
            .ToListAsync();

        return new PagedResult<Product> { Items = items, Total = total, Page = page, PageSize = query.PageSize };
    }

    public Task<Product?> GetDetailAsync(int id) =>
        Db.Products.AsNoTracking()
            .Include(p => p.Series)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specs.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetForEditAsync(int id) =>
        Db.Products
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specs.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetByModelAsync(string model) =>
        Db.Products.Include(p => p.Specs).FirstOrDefaultAsync(p => p.Model == model);

    public Task<Product?> GetByIdAsync(int id) => Db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        return await Db.Products.AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .OrderBy(p => p.Model)
            .ToListAsync();
    }

    /// <summary>产品选择器轻量搜索（意向单添加产品用）。</summary>
    public Task<List<Product>> SearchForPickerAsync(string keyword, int limit = 20)
    {
        var kw = keyword.Trim();
        return Db.Products.AsNoTracking()
            .Where(p => p.IsActive && (p.Model.Contains(kw) || p.Name.Contains(kw)))
            .OrderBy(p => p.Model)
            .Take(limit)
            .Select(p => new Product { Id = p.Id, Model = p.Model, Name = p.Name })
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = product.CreatedAt;
        Db.Products.Add(product);
        await Db.SaveChangesAsync();
        return product;
    }

    public Task SaveAsync(Product product)
    {
        product.UpdatedAt = DateTime.Now;
        return Db.SaveChangesAsync();
    }

    public async Task RemoveProductAsync(Product product, string webRootPath)
    {
        foreach (var image in product.Images)
        {
            var file = Path.Combine(webRootPath, image.Path.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(file)) File.Delete(file);
        }
        Db.Products.Remove(product);
        await Db.SaveChangesAsync();
    }

    public Task<int> CountAsync(int? seriesId = null)
    {
        var q = Db.Products.AsNoTracking().Where(p => p.IsActive);
        if (seriesId is int sid) q = q.Where(p => p.SeriesId == sid);
        return q.CountAsync();
    }
}
