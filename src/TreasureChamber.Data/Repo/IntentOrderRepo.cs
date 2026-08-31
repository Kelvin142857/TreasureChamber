using Microsoft.EntityFrameworkCore;
using TreasureChamber.Core.Entities;
using TreasureChamber.Core.Enums;
using TreasureChamber.Core.Models;

namespace TreasureChamber.Data.Repo;

public class IntentOrderRepo(AppDbContext db) : BaseRepo(db)
{
    public async Task<PagedResult<IntentOrder>> QueryAsync(IntentOrderStatus? status, string? keyword, int page, int pageSize = 20)
    {
        var q = Db.IntentOrders.AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();
        if (status is IntentOrderStatus s) q = q.Where(o => o.Status == s);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            q = q.Where(o => o.OrderNo.Contains(kw) || o.CustomerName.Contains(kw) || o.CustomerPhone.Contains(kw));
        }
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PagedResult<IntentOrder> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public Task<IntentOrder?> GetDetailAsync(int id) =>
        Db.IntentOrders.AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

    public Task<IntentOrder?> GetForEditAsync(int id) =>
        Db.IntentOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IntentOrder> AddAsync(IntentOrder order)
    {
        order.CreatedAt = DateTime.Now;
        Db.IntentOrders.Add(order);
        await Db.SaveChangesAsync();
        return order;
    }

    public Task SaveAsync(IntentOrder order) => Db.SaveChangesAsync();

    public async Task UpdateStatusAsync(int id, IntentOrderStatus status)
    {
        var order = await Db.IntentOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order != null)
        {
            order.Status = status;
            await Db.SaveChangesAsync();
        }
    }

    /// <summary>导出用：全部意向单（含明细与产品型号）。</summary>
    public Task<List<IntentOrder>> GetAllForExportAsync() =>
        Db.IntentOrders.AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<bool> OrderNoExistsAsync(string orderNo) =>
        await Db.IntentOrders.AsNoTracking().AnyAsync(o => o.OrderNo == orderNo);
}
