using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data;

/// <summary>建库 + 首次运行种子数据。</summary>
public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Categories.Any() || db.Series.Any()) return;

        var series = new List<Series>
        {
            new() { Name = "现代简约", SortOrder = 1 },
            new() { Name = "北欧风格", SortOrder = 2 },
            new() { Name = "轻奢风", SortOrder = 3 }
        };
        db.Series.AddRange(series);

        var lighting = new Category { Name = "灯具", SortOrder = 1 };
        db.Categories.Add(lighting);
        var pendants = new Category { Name = "吊灯", Parent = lighting, SortOrder = 1 };
        var tableLamps = new Category { Name = "台灯", Parent = lighting, SortOrder = 2 };
        var wallLamps = new Category { Name = "壁灯", Parent = lighting, SortOrder = 3 };
        var ceiling = new Category { Name = "吸顶灯", Parent = lighting, SortOrder = 4 };
        db.Categories.AddRange(pendants, tableLamps, wallLamps, ceiling);
        db.SaveChanges();

        var now = DateTime.Now;
        db.Products.AddRange(
            new Product
            {
                Model = "XD-1001", Name = "云朵吊灯", Series = series[0], Category = pendants,
                Description = "直径 60cm 极简白色云朵造型吊灯，适合客厅与卧室，柔和漫射光不刺眼。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "36W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "3000K-5000K 三档可调", SortOrder = 2 },
                    new ProductSpec { Name = "显色指数", Value = "Ra≥90", SortOrder = 3 },
                    new ProductSpec { Name = "材质", Value = "铁艺+亚克力", SortOrder = 4 },
                    new ProductSpec { Name = "光源", Value = "LED 模组", SortOrder = 5 }
                }
            },
            new Product
            {
                Model = "XD-1002", Name = "圆环吊灯", Series = series[2], Category = pendants,
                Description = "黄铜金属圆环吊灯，轻奢质感，适用于餐厅与吧台区域。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "24W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "3000K", SortOrder = 2 },
                    new ProductSpec { Name = "材质", Value = "黄铜+玻璃", SortOrder = 3 }
                }
            },
            new Product
            {
                Model = "TD-2001", Name = "护眼阅读台灯", Series = series[0], Category = tableLamps,
                Description = "无频闪护眼台灯，触控调光，适合书房阅读与办公。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "12W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "2700K-6000K 无极调光", SortOrder = 2 },
                    new ProductSpec { Name = "显色指数", Value = "Ra≥95", SortOrder = 3 },
                    new ProductSpec { Name = "控制方式", Value = "触控", SortOrder = 4 }
                }
            },
            new Product
            {
                Model = "TD-2002", Name = "原木床头台灯", Series = series[1], Category = tableLamps,
                Description = "天然原木灯座，布艺灯罩，温馨北欧风，适合卧室床头。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "8W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "3000K", SortOrder = 2 },
                    new ProductSpec { Name = "材质", Value = "原木+棉麻布", SortOrder = 3 }
                }
            },
            new Product
            {
                Model = "BD-3001", Name = "极简壁灯", Series = series[0], Category = wallLamps,
                Description = "隐藏光源上照式壁灯，洗墙效果，适合走廊与床头。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "9W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "4000K", SortOrder = 2 },
                    new ProductSpec { Name = "安装方式", Value = "壁挂", SortOrder = 3 }
                }
            },
            new Product
            {
                Model = "XDD-4001", Name = "超薄吸顶灯", Series = series[0], Category = ceiling,
                Description = "8cm 超薄全光谱吸顶灯，支持遥控调光调色，适合卧室。",
                IsActive = true, CreatedAt = now, UpdatedAt = now,
                Specs =
                {
                    new ProductSpec { Name = "功率", Value = "48W", SortOrder = 1 },
                    new ProductSpec { Name = "色温", Value = "2700K-5700K 遥控调节", SortOrder = 2 },
                    new ProductSpec { Name = "厚度", Value = "8cm", SortOrder = 3 },
                    new ProductSpec { Name = "适用面积", Value = "15-20㎡", SortOrder = 4 }
                }
            });
        db.SaveChanges();
    }
}
