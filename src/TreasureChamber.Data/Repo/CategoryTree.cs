using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data.Repo;

/// <summary>分类树工具。</summary>
public static class CategoryTree
{
    /// <summary>返回 selfId 自身及所有子孙分类的 Id 集合。</summary>
    public static HashSet<int> DescendantIdsIncludingSelf(this List<Category> all, int selfId)
    {
        var result = new HashSet<int> { selfId };
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var c in all)
            {
                if (c.ParentId is int pid && result.Contains(pid) && result.Add(c.Id)) changed = true;
            }
        }
        return result;
    }

    /// <summary>构建分类树（带每节点产品数量，来自传入的计数映射）。</summary>
    public static List<CategoryNode> BuildTree(this List<Category> all, IReadOnlyDictionary<int, int>? counts = null)
    {
        var nodes = all.ToDictionary(c => c.Id, c => new CategoryNode { Id = c.Id, ParentId = c.ParentId, Name = c.Name, Count = counts?.GetValueOrDefault(c.Id) ?? 0 });
        var roots = new List<CategoryNode>();
        foreach (var n in nodes.Values)
        {
            if (n.ParentId is int pid && nodes.ContainsKey(pid)) nodes[pid].Children.Add(n);
            else roots.Add(n);
        }
        return roots.OrderBy(n => n.Name).ToList();
    }
}

public class CategoryNode
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
    public List<CategoryNode> Children { get; set; } = new();
}
