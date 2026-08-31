using TreasureChamber.Application.Dtos;
using TreasureChamber.Core.Entities;
using TreasureChamber.Core.Enums;
using TreasureChamber.Data.Repo;

namespace TreasureChamber.Application.Services;

/// <summary>意向单创建（编号生成 + 产品快照）。</summary>
public class IntentOrderService(IntentOrderRepo repo, ProductRepo productRepo)
{
    public async Task<(IntentOrder? Order, string? Error)> CreateAsync(IntentOrderDraft draft)
    {
        if (string.IsNullOrWhiteSpace(draft.CustomerName))
            return (null, "请填写客户姓名");
        if (string.IsNullOrWhiteSpace(draft.CustomerPhone))
            return (null, "请填写客户电话");

        var validItems = draft.Items
            .Where(i => i.Quantity > 0 && (i.ProductId != null || !string.IsNullOrWhiteSpace(i.Model)))
            .ToList();
        if (validItems.Count == 0)
            return (null, "请至少选择一款产品并填写数量");

        var order = new IntentOrder
        {
            OrderNo = await NextOrderNoAsync(),
            CustomerName = draft.CustomerName.Trim(),
            CustomerPhone = draft.CustomerPhone.Trim(),
            CustomerCompany = string.IsNullOrWhiteSpace(draft.CustomerCompany) ? null : draft.CustomerCompany.Trim(),
            Note = string.IsNullOrWhiteSpace(draft.Note) ? null : draft.Note.Trim(),
            Status = IntentOrderStatus.New
        };

        foreach (var item in validItems)
        {
            var product = item.ProductId is int pid
                ? await productRepo.GetByIdAsync(pid)
                : await productRepo.GetByModelAsync(item.Model.Trim());
            if (product == null) continue;

            order.Items.Add(new IntentOrderItem
            {
                ProductId = product.Id,
                ProductModel = product.Model,
                ProductName = product.Name,
                Quantity = item.Quantity,
                Remark = string.IsNullOrWhiteSpace(item.Remark) ? null : item.Remark.Trim()
            });
        }

        if (order.Items.Count == 0)
            return (null, "所选产品不存在，请重新选择");

        await repo.AddAsync(order);
        return (order, null);
    }

    private async Task<string> NextOrderNoAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            var orderNo = "YX" + DateTime.Now.ToString("yyyyMMddHHmmss") + Random.Shared.Next(10, 99);
            if (!await repo.OrderNoExistsAsync(orderNo)) return orderNo;
        }
        return "YX" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }
}
