using Microsoft.EntityFrameworkCore;
using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data.Repo;

public class CategoryRepo(AppDbContext db) : BaseRepo(db)
{
    public Task<List<Category>> GetAllAsync() =>
        Db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();

    /// <summary>按名称查找，不存在则创建（导入时自动建分类）。</summary>
    public async Task<Category> EnsureAsync(string name)
    {
        var existed = await Db.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (existed != null) return existed;
        var created = new Category { Name = name, SortOrder = 0 };
        Db.Categories.Add(created);
        await Db.SaveChangesAsync();
        return created;
    }

    /// <summary>每个分类（含子孙）下的在售产品数量。</summary>
    public async Task<Dictionary<int, int>> ProductCountsAsync()
    {
        var categories = await Db.Categories.AsNoTracking().ToListAsync();
        var raw = await Db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.CategoryId != null)
            .GroupBy(p => p.CategoryId!.Value)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        var counts = new Dictionary<int, int>();
        foreach (var r in raw) counts[r.Key] = r.Count;
        // 累加到祖先
        var totals = categories.ToDictionary(c => c.Id, c => 0);
        foreach (var r in raw) totals[r.Key] = r.Count;
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var c in categories)
            {
                if (c.ParentId is int pid && totals.ContainsKey(pid) && totals[c.Id] > 0)
                {
                    totals[pid] += totals[c.Id];
                    totals[c.Id] = 0;
                    changed = true;
                }
            }
        }
        return totals;
    }
}
