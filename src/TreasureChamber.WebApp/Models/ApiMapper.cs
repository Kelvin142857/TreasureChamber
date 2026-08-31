using TreasureChamber.Core.Entities;
using TreasureChamber.Data.Repo;

namespace TreasureChamber.WebApp.Models;

/// <summary>实体 → API DTO 映射。</summary>
public static class ApiMapper
{
    public static ProductCardDto ToCard(Product p) => new(
        p.Id, p.Model, p.Name,
        p.Series?.Name, p.Category?.Name,
        p.Images.FirstOrDefault() is { } img ? img.Path : null);

    public static ProductDetailDto ToDetail(Product p) => new(
        p.Id, p.Model, p.Name, p.Description,
        p.SeriesId, p.Series?.Name, p.CategoryId, p.Category?.Name, p.IsActive, p.UpdatedAt,
        p.Images.Select(i => new ImageDto(i.Id, i.Path, i.SortOrder)).ToList(),
        p.Specs.Select(s => new SpecDto(s.Id, s.Name, s.Value)).ToList());

    public static CategoryNodeDto ToTree(CategoryNode node) => new(
        node.Id, node.Name, node.Count,
        node.Children.Select(ToTree).ToList());

    public static List<CategoryNodeDto> ToTree(List<CategoryNode> nodes) =>
        nodes.Select(ToTree).ToList();

    public static IntentOrderDto ToIntentOrder(IntentOrder o) => new(
        o.Id, o.OrderNo, o.CustomerName, o.CustomerPhone, o.CustomerCompany, o.Note,
        (int)o.Status, Core.Enums.IntentOrderStatusLabels.All[o.Status], o.CreatedAt,
        o.Items.Select(i => new IntentOrderItemViewDto(i.Id, i.ProductId, i.ProductModel, i.ProductName, i.Quantity, i.Remark)).ToList());
}
