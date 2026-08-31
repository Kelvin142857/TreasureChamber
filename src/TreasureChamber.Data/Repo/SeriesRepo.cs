using Microsoft.EntityFrameworkCore;
using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data.Repo;

public class SeriesRepo(AppDbContext db) : BaseRepo(db)
{
    public Task<List<Series>> GetAllAsync() =>
        Db.Series.AsNoTracking().OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToListAsync();

    public async Task<Series> EnsureAsync(string name)
    {
        var existed = await Db.Series.FirstOrDefaultAsync(s => s.Name == name);
        if (existed != null) return existed;
        var created = new Series { Name = name, SortOrder = 0 };
        Db.Series.Add(created);
        await Db.SaveChangesAsync();
        return created;
    }

    public async Task<List<SeriesWithCount>> WithCountsAsync()
    {
        var series = await Db.Series.AsNoTracking()
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .Select(s => new SeriesWithCount { Id = s.Id, Name = s.Name })
            .ToListAsync();
        var counts = await Db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.SeriesId != null)
            .GroupBy(p => p.SeriesId!.Value)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        foreach (var s in series) s.Count = counts.GetValueOrDefault(s.Id);
        return series;
    }
}

public class SeriesWithCount
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
}
