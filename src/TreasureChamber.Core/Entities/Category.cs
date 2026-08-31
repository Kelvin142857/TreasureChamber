namespace TreasureChamber.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
    public List<Category> Children { get; set; } = new();
}
