namespace TreasureChamber.Core.Models;

public class ProductQuery
{
    public int? SeriesId { get; set; }
    public int? CategoryId { get; set; }
    /// <summary>按型号/名称模糊搜索。</summary>
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
