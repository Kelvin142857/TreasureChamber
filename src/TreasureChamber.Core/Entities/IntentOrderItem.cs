namespace TreasureChamber.Core.Entities;

public class IntentOrderItem
{
    public int Id { get; set; }
    public int IntentOrderId { get; set; }
    public IntentOrder IntentOrder { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>产品快照（型号/名称），产品被删除后意向单仍可读。</summary>
    public string ProductModel { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public string? Remark { get; set; }
}
