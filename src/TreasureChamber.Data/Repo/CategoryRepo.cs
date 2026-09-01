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
        var totals = categories.ToDictionary(c => c.Id, c => 0);
        foreach (var r in raw) totals[r.Key] = r.Count;

        // 求每个节点的深度（父先于子），再自底向上累加：每节点数量 = 自身 + 全部子孙
        var depth = categories.ToDictionary(c => c.Id, c => 0);
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var c in categories)
            {
                if (c.ParentId is int pid && depth.ContainsKey(pid) && depth[c.Id] <= depth[pid])
                {
                    depth[c.Id] = depth[pid] + 1;
                    changed = true;
                }
            }
        }
        foreach (var c in categories.OrderByDescending(c => depth.GetValueOrDefault(c.Id)))
        {
            if (c.ParentId is int pid && totals.ContainsKey(pid))
                totals[pid] += totals[c.Id];
        }
        return totals;
    }
}
