using TreasureChamber.Core.Entities;

namespace TreasureChamber.Data;

/// <summary>建库 + 首次运行种子数据（展厅模拟商品）。</summary>
public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Categories.Any() || db.Series.Any()) return;

        // ===== 系列 =====
        var modern = new Series { Name = "现代简约", SortOrder = 1 };
        var nordic = new Series { Name = "北欧风格", SortOrder = 2 };
        var luxury = new Series { Name = "轻奢风", SortOrder = 3 };
        var chinese = new Series { Name = "新中式", SortOrder = 4 };
        var industrial = new Series { Name = "工业风", SortOrder = 5 };
        var smart = new Series { Name = "智能家居", SortOrder = 6 };
        db.Series.AddRange(modern, nordic, luxury, chinese, industrial, smart);

        // ===== 分类树 =====
        var lighting = new Category { Name = "灯具", SortOrder = 1 };
        var pendants = new Category { Name = "吊灯", Parent = lighting, SortOrder = 1 };
        var ceiling = new Category { Name = "吸顶灯", Parent = lighting, SortOrder = 2 };
        var downlights = new Category { Name = "筒灯", Parent = lighting, SortOrder = 3 };
        var spots = new Category { Name = "射灯", Parent = lighting, SortOrder = 4 };
        var tableLamps = new Category { Name = "台灯", Parent = lighting, SortOrder = 5 };
        var floorLamps = new Category { Name = "落地灯", Parent = lighting, SortOrder = 6 };
        var wallLamps = new Category { Name = "壁灯", Parent = lighting, SortOrder = 7 };
        var strips = new Category { Name = "灯带", Parent = lighting, SortOrder = 8 };
        db.Categories.AddRange(lighting, pendants, ceiling, downlights, spots, tableLamps, floorLamps, wallLamps, strips);
        db.SaveChanges();

        var now = DateTime.Now;
        var products = new List<Product>
        {
            // ================= 吊灯 =================
            P("XD-1001", "云朵造型吊灯", modern, pendants, now,
                "直径 60cm 极简白色云朵造型吊灯，亚克力漫射灯罩光线柔和均匀不刺眼，适合客厅与卧室主照明。",
                ("功率", "36W"), ("色温", "3000K-5000K 三档可调"), ("显色指数", "Ra≥90"),
                ("尺寸", "Φ600×220mm"), ("材质", "铁艺+亚克力"), ("光源类型", "LED 集成模组")),
            P("XD-1002", "黄铜圆环吊灯", luxury, pendants, now,
                "黄铜金属圆环搭配磨砂玻璃灯罩，轻奢质感，光线层次丰富，适用于餐厅与吧台区域。",
                ("功率", "24W"), ("色温", "3000K 暖光"), ("显色指数", "Ra≥95"),
                ("尺寸", "Φ500×600mm"), ("材质", "H59 黄铜+玻璃"), ("灯头数量", "1 头")),
            P("XD-1003", "分子结构吊灯", industrial, pendants, now,
                "多球体分子造型，黑色金属灯臂可多角度调节，工业复古风格，适合 loft 客厅与书房。",
                ("功率", "40W（10×4W E14）"), ("色温", "2700K"), ("灯头数量", "10 头 E14"),
                ("尺寸", "900×800mm"), ("材质", "铁艺"), ("光源类型", "可换灯泡")),
            P("XD-1004", "隐形风扇吊灯", smart, pendants, now,
                "吊扇与照明二合一，直流变频电机静音运行，遥控/壁控双控制，夏天照明降温两不误。",
                ("功率", "照明 36W + 风扇 30W"), ("色温", "3000K-6000K 无极调光调色"),
                ("控制方式", "遥控器+墙壁开关"), ("风量档位", "6 档"), ("噪音", "≤35dB"),
                ("尺寸", "Φ1320mm"), ("材质", "铝合金+亚克力")),
            P("XD-1005", "餐厅三头吊灯", nordic, pendants, now,
                "北欧原木三头吊灯，橡木灯身配奶白玻璃罩，光线温馨聚拢，餐桌氛围首选。",
                ("功率", "27W（3×9W）"), ("色温", "3000K"), ("显色指数", "Ra≥90"),
                ("尺寸", "L900×350mm"), ("材质", "橡木+玻璃"), ("安装方式", "吊线可调 0.5-1.2m")),

            // ================= 吸顶灯 =================
            P("XDD-2001", "超薄全光谱吸顶灯", modern, ceiling, now,
                "8cm 超薄灯体贴顶不压抑，全光谱 LED 接近自然光，支持遥控调光调色，适合卧室。",
                ("功率", "48W"), ("色温", "2700K-5700K 遥控调节"), ("显色指数", "Ra≥97 全光谱"),
                ("尺寸", "Φ500×80mm"), ("适用面积", "15-20㎡"), ("控制方式", "遥控器"),
                ("材质", "铝材+亚克力")),
            P("XDD-2002", "北欧木质吸顶灯", nordic, ceiling, now,
                "实木边框圆形吸顶灯，原木纹理温润自然，适合儿童房与卧室，光线柔和不频闪。",
                ("功率", "36W"), ("色温", "4000K 中性光"), ("显色指数", "Ra≥95"),
                ("尺寸", "Φ450×95mm"), ("材质", "榉木+亚克力"), ("防护等级", "IP20")),
            P("XDD-2003", "智能语音吸顶灯", smart, ceiling, now,
                "支持米家/小爱同学语音控制与 APP 远程调节，无极调光调色，可接入智能家居联动场景。",
                ("功率", "60W"), ("色温", "2700K-6500K 无极调节"), ("控制方式", "语音+APP+遥控"),
                ("协议", "米家蓝牙 Mesh"), ("尺寸", "Φ560×88mm"), ("适用面积", "20-25㎡")),
            P("XDD-2004", "防潮卫生间吸顶灯", modern, ceiling, now,
                "IP54 防尘防潮设计，密闭灯体隔绝水汽，适合卫生间、厨房与阳台照明。",
                ("功率", "24W"), ("色温", "4000K"), ("防护等级", "IP54 防尘防潮"),
                ("尺寸", "Φ300×78mm"), ("材质", "阻燃 PC"), ("质保", "3 年")),

            // ================= 筒灯 =================
            P("TD-3001", "防眩深藏筒灯", modern, downlights, now,
                "60° 防眩光学设计，深藏光源不刺眼，客厅无主灯照明基础光源，见光不见灯。",
                ("功率", "9W"), ("色温", "4000K"), ("显色指数", "Ra≥90"),
                ("开孔尺寸", "Φ75-80mm"), ("光束角", "60°"), ("防眩等级", "UGR<19"),
                ("材质", "压铸铝")),
            P("TD-3002", "COB 深防筒灯", luxury, downlights, now,
                "COB 光源配蜂窝防眩网，双层防眩结构，光线细腻均匀，适合卧室与走道。",
                ("功率", "12W"), ("色温", "3000K"), ("显色指数", "Ra≥95"),
                ("开孔尺寸", "Φ90-95mm"), ("光束角", "38°"), ("材质", "航空铝+蜂窝网")),
            P("TD-3003", "明装轨道筒灯", industrial, downlights, now,
                "免开孔明装设计，黑色工业风灯体，顶面不打孔即装，适合租房与旧房改造。",
                ("功率", "7W"), ("色温", "3000K/4000K 可选"), ("安装方式", "明装免开孔"),
                ("尺寸", "Φ92×115mm"), ("材质", "铁艺"), ("光束角", "24°")),

            // ================= 射灯 =================
            P("SD-4001", "格栅轨道射灯", industrial, spots, now,
                "轨道格栅射灯，工业感黑色灯体，角度可调，服装店与背景墙重点照明利器。",
                ("功率", "10W"), ("色温", "3000K"), ("显色指数", "Ra≥90"),
                ("轨道", "三线轨道"), ("调节角度", "水平 350°/垂直 90°"), ("材质", "铝材")),
            P("SD-4002", "磁吸轨道射灯", modern, spots, now,
                "磁吸快拆设计，随手一贴即亮，可任意增减灯体组合，客厅无主灯方案首选。",
                ("功率", "9W"), ("色温", "3500K"), ("显色指数", "Ra≥95"),
                ("轨道", "磁吸 48V 低压"), ("安装方式", "吸顶/吊线两用"), ("调节角度", "垂直 25°")),
            P("SD-4003", "可调角洗墙射灯", luxury, spots, now,
                "30° 可调角设计，防眩加深反光杯，画作与背景墙洗墙效果出众。",
                ("功率", "7W"), ("色温", "3000K 暖白"), ("光束角", "36°"),
                ("开孔尺寸", "Φ75mm"), ("材质", "压铸铝"), ("防眩等级", "UGR<16")),

            // ================= 台灯 =================
            P("TT-5001", "国AA级护眼台灯", modern, tableLamps, now,
                "国 AA 级照度认证，无频闪无蓝光危害，触控无极调光，学生学习与办公护眼首选。",
                ("功率", "12W"), ("色温", "2700K-6000K 无极调节"), ("照度等级", "国 AA 级"),
                ("显色指数", "Ra≥98"), ("控制方式", "触控+定时"), ("频闪", "无可视频闪（豁免级）")),
            P("TT-5002", "原木床头台灯", nordic, tableLamps, now,
                "天然橡木灯座配棉麻布灯罩，暖光透出温润质感，卧室床头氛围灯。",
                ("功率", "8W E27"), ("色温", "2700K"), ("材质", "橡木+棉麻"),
                ("尺寸", "Φ260×450mm"), ("开关", "线控"), ("光源类型", "可换灯泡")),
            P("TT-5003", "氛围光立方台灯", smart, tableLamps, now,
                "16 万色 RGB 氛围灯，APP/语音控制，音乐律动模式，游戏房与直播间氛围利器。",
                ("功率", "10W"), ("色温", "RGB 1600 万色"), ("控制方式", "APP+语音+遥控"),
                ("特效", "音乐律动/日出唤醒"), ("尺寸", "160×160×160mm"), ("供电", "DC 12V")),

            // ================= 落地灯 =================
            P("LD-6001", "钓鱼弧形落地灯", modern, floorLamps, now,
                "大弧度钓鱼杆造型，光源悬于沙发上方不占空间，阅读与氛围兼顾。",
                ("功率", "18W"), ("色温", "3000K-4500K 可调"), ("显色指数", "Ra≥90"),
                ("高度", "1950mm"), ("弧臂跨度", "950mm"), ("底座", "大理石配重")),
            P("LD-6002", "北欧三脚落地灯", nordic, floorLamps, now,
                "实木三脚支架配圆筒布罩，客厅角落颜值担当，光线温暖柔和。",
                ("功率", "12W E27"), ("色温", "2700K"), ("材质", "橡胶木+亚麻布"),
                ("高度", "1600mm"), ("开关", "脚踏开关"), ("光源类型", "可换灯泡")),

            // ================= 壁灯 =================
            P("BD-7001", "极简上照壁灯", modern, wallLamps, now,
                "隐藏光源上照式设计，洗墙光晕优雅不刺眼，走廊与床头氛围照明。",
                ("功率", "9W"), ("色温", "3000K"), ("光束角", "上照 100°"),
                ("尺寸", "180×80×90mm"), ("安装方式", "壁挂嵌入式接线"), ("材质", "铝材")),
            P("BD-7002", "智能镜前壁灯", smart, wallLamps, now,
                "梳妆镜前专用高显指壁灯，无频闪化妆补光，支持三档色温切换。",
                ("功率", "12W"), ("色温", "3500K/4500K/6000K 三档"), ("显色指数", "Ra≥95"),
                ("尺寸", "L600×65mm"), ("控制方式", "触摸开关"), ("防水", "IP44")),

            // ================= 灯带 =================
            P("DD-8001", "COB 无暗区灯带", modern, strips, now,
                "COB 封装灯珠密集无光斑，背部导热胶散热好，吊顶暗藏与柜内照明首选。",
                ("功率", "10W/m"), ("色温", "3000K/4000K 可选"), ("显色指数", "Ra≥90"),
                ("宽度", "8mm"), ("剪切单位", "每 25mm 可剪"), ("防护等级", "IP20/滴胶 IP65 可选")),
            P("DD-8002", "硅胶霓虹灯带", luxury, strips, now,
                "侧发光硅胶霓虹灯带，可弯曲造型勾勒轮廓，户外招牌与背景墙均适用。",
                ("功率", "8W/m"), ("色温", "2700K 暖光"), ("弯曲直径", "最小 60mm"),
                ("防护等级", "IP67 户外可用"), ("材质", "食品级硅胶"), ("长度规格", "5m/卷")),

            // ================= 新中式补充 =================
            P("XD-1006", "禅意圆形新中式吊灯", chinese, pendants, now,
                "圆形水墨意境灯罩配胡桃木框，禅意东方美学，茶室与书房气质之选。",
                ("功率", "32W"), ("色温", "3000K"), ("显色指数", "Ra≥92"),
                ("尺寸", "Φ560×300mm"), ("材质", "胡桃木+亚麻布"), ("光源类型", "LED 模组")),
            P("BD-7003", "新中式云石壁灯", chinese, wallLamps, now,
                "仿云石灯罩纹理通透，光线如月光温润，玄关与走廊点缀照明。",
                ("功率", "6W"), ("色温", "2700K"), ("材质", "仿云石树脂+铜"),
                ("尺寸", "Φ160×260mm"), ("安装方式", "壁挂"), ("防护等级", "IP20"))
        };
        db.Products.AddRange(products);
        db.SaveChanges();
    }

    private static Product P(string model, string name, Series series, Category category,
        DateTime now, string description, params (string Name, string Value)[] specs)
    {
        var product = new Product
        {
            Model = model,
            Name = name,
            Series = series,
            Category = category,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        var order = 0;
        foreach (var (name2, value) in specs)
        {
            product.Specs.Add(new ProductSpec { Name = name2, Value = value, SortOrder = ++order });
        }
        return product;
    }
}
