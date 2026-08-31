namespace TreasureChamber.Core.Enums;

public enum IntentOrderStatus
{
    /// <summary>新建</summary>
    New = 0,
    /// <summary>跟进中</summary>
    Following = 1,
    /// <summary>已成交</summary>
    Deal = 2,
    /// <summary>已放弃</summary>
    Abandoned = 3
}

public static class IntentOrderStatusLabels
{
    public static readonly Dictionary<IntentOrderStatus, string> All = new()
    {
        [IntentOrderStatus.New] = "新建",
        [IntentOrderStatus.Following] = "跟进中",
        [IntentOrderStatus.Deal] = "已成交",
        [IntentOrderStatus.Abandoned] = "已放弃"
    };
}
