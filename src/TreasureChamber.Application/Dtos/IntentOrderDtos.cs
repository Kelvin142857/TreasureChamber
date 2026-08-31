namespace TreasureChamber.Application.Dtos;

public class IntentOrderDraft
{
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string? CustomerCompany { get; set; }
    public string? Note { get; set; }
    public List<IntentOrderItemDraft> Items { get; set; } = new();
}

public class IntentOrderItemDraft
{
    public int? ProductId { get; set; }
    /// <summary>产品型号（未选 ID 时按型号精确匹配）。</summary>
    public string Model { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public string? Remark { get; set; }
}
