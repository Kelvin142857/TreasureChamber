namespace TreasureChamber.Core.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    /// <summary>相对 wwwroot 的路径，如 uploads/1/xxx.jpg。</summary>
    public string Path { get; set; } = "";
    public int SortOrder { get; set; }
}
