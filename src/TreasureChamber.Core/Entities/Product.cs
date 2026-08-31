namespace TreasureChamber.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Model { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? SeriesId { get; set; }
    public Series? Series { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductSpec> Specs { get; set; } = new();
}
