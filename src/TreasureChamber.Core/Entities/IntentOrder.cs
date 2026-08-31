using TreasureChamber.Core.Enums;

namespace TreasureChamber.Core.Entities;

public class IntentOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string? CustomerCompany { get; set; }
    public string? Note { get; set; }
    public IntentOrderStatus Status { get; set; } = IntentOrderStatus.New;
    public DateTime CreatedAt { get; set; }
    public List<IntentOrderItem> Items { get; set; } = new();
}
