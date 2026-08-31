using System.Text;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using TreasureChamber.Application.Dtos;
using TreasureChamber.Core.Entities;
using TreasureChamber.Data;
using TreasureChamber.Data.Repo;

namespace TreasureChamber.Application.Services;

/// <summary>
/// 产品目录批量导入：支持 Excel(.xlsx) 与 CSV。
/// 表头：型号*、名称*、系列、分类（支持 父/子 层级）、描述、规格参数（格式：名称=值;名称=值）。
/// </summary>
public class ProductImportService(AppDbContext db, CategoryRepo categoryRepo, SeriesRepo seriesRepo)
{
    private static readonly string[] HeaderModel = ["型号", "名称", "系列", "分类", "描述", "规格参数"];

    public static bool IsSupportedFile(string fileName) =>
        fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    /// <summary>生成可下载的导入模板（xlsx）。</summary>
    public byte[] CreateTemplate()
    {
        var sample = new Dictionary<string, object?>
        {
            ["型号"] = "XD-3000",
            ["名称"] = "示例吊灯",
            ["系列"] = "现代简约",
            ["分类"] = "灯具/吊灯",
            ["描述"] = "这里是产品介绍文字，可多行。",
            ["规格参数"] = "功率=36W;色温=3000K;显色指数=Ra≥90"
        };
        using var ms = new MemoryStream();
        MiniExcel.SaveAs(ms, new[] { sample }, sheetName: "产品导入");
        return ms.ToArray();
    }

    /// <summary>解析文件用于预览（不写库）。</summary>
    public async Task<ImportPreview> ParseAsync(string filePath, string fileName)
    {
        var rows = new List<ImportRow>();
        var existingModels = (await db.Products.AsNoTracking().Select(p => p.Model).ToListAsync()).ToHashSet();

        foreach (var (rowNumber, fields) in ReadRows(filePath))
        {
            var row = new ImportRow { RowNumber = rowNumber };
            var model = Get(fields, "型号");
            var name = Get(fields, "名称");

            if (string.IsNullOrWhiteSpace(model))
            {
                row.Action = ImportAction.Error;
                row.Error = "型号不能为空";
                rows.Add(row);
                continue;
            }
            row.Model = model.Trim();
            row.Name = string.IsNullOrWhiteSpace(name) ? row.Model : name.Trim();
            row.Series = NullIfBlank(Get(fields, "系列"));
            row.Category = NullIfBlank(Get(fields, "分类"));
            row.Description = NullIfBlank(Get(fields, "描述"));
            row.Specs = ParseSpecs(Get(fields, "规格参数"));
            row.Action = existingModels.Contains(row.Model) ? ImportAction.Update : ImportAction.New;
            rows.Add(row);
        }
        return new ImportPreview { FileName = fileName, Rows = rows };
    }

    /// <summary>执行入库：系列/分类不存在自动创建，型号已存在则更新。</summary>
    public async Task<ImportResult> CommitAsync(string filePath)
    {
        var result = new ImportResult();
        var existingModels = (await db.Products.AsNoTracking().Select(p => p.Model).ToListAsync()).ToHashSet();

        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var (_, fields) in ReadRows(filePath))
        {
            var model = Get(fields, "型号")?.Trim();
            if (string.IsNullOrWhiteSpace(model))
            {
                result.Skipped++;
                continue;
            }
            var name = string.IsNullOrWhiteSpace(Get(fields, "名称")) ? model : Get(fields, "名称")!.Trim();
            var seriesName = NullIfBlank(Get(fields, "系列"));
            var categoryPath = NullIfBlank(Get(fields, "分类"));
            var description = NullIfBlank(Get(fields, "描述"));
            var specs = ParseSpecs(Get(fields, "规格参数"));

            var seriesId = seriesName == null ? null : (int?)(await seriesRepo.EnsureAsync(seriesName)).Id;
            var categoryId = categoryPath == null ? null : (int?)(await EnsureCategoryByPathAsync(categoryPath)).Id;

            var product = await db.Products.Include(p => p.Specs).FirstOrDefaultAsync(p => p.Model == model);
            if (product == null)
            {
                product = new Product
                {
                    Model = model,
                    Name = name,
                    Description = description,
                    SeriesId = seriesId,
                    CategoryId = categoryId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                db.Products.Add(product);
                await db.SaveChangesAsync();
                result.Added++;
            }
            else
            {
                product.Name = name;
                product.Description = description;
                product.SeriesId = seriesId;
                product.CategoryId = categoryId;
                product.UpdatedAt = DateTime.Now;
                db.ProductSpecs.RemoveRange(product.Specs);
                result.Updated++;
            }
            var order = 0;
            foreach (var spec in specs)
            {
                db.ProductSpecs.Add(new ProductSpec { ProductId = product.Id, Name = spec.Key, Value = spec.Value, SortOrder = ++order });
            }
            await db.SaveChangesAsync();
        }
        await tx.CommitAsync();
        return result;
    }

    private async Task<Category> EnsureCategoryByPathAsync(string path)
    {
        Category? parent = null;
        Category? current = null;
        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            current = parent == null
                ? await categoryRepo.EnsureAsync(part)
                : await db.Categories.FirstOrDefaultAsync(c => c.ParentId == parent.Id && c.Name == part)
                  ?? await CreateChildAsync(parent, part);
            parent = current;
        }
        return current!;
    }

    private async Task<Category> CreateChildAsync(Category parent, string name)
    {
        var child = new Category { ParentId = parent.Id, Name = name, SortOrder = 0 };
        db.Categories.Add(child);
        await db.SaveChangesAsync();
        return child;
    }

    private static List<KeyValuePair<string, string>> ParseSpecs(string? text)
    {
        var result = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        foreach (var part in text.Split([';', '；'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var sep = part.IndexOfAny(['=', '：']);
            if (sep <= 0) continue;
            var key = part[..sep].Trim();
            var value = part[(sep + 1)..].Trim();
            if (key.Length > 0) result.Add(new KeyValuePair<string, string>(key, value));
        }
        return result;
    }

    private static string? Get(IReadOnlyDictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out var v) ? v : null;

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>按扩展名读取行，统一输出 (行号, 列名→值)。</summary>
    private static IEnumerable<(int RowNumber, Dictionary<string, string> Fields)> ReadRows(string filePath)
    {
        if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return ReadCsv(filePath);
        return ReadExcel(filePath);
    }

    private static IEnumerable<(int RowNumber, Dictionary<string, string> Fields)> ReadExcel(string filePath)
    {
        var rows = MiniExcel.Query(filePath, useHeaderRow: true).ToList();
        var result = new List<(int, Dictionary<string, string>)>();
        var rowNumber = 1;
        foreach (var row in rows)
        {
            rowNumber++;
            var fields = new Dictionary<string, string>();
            foreach (var kv in row as IDictionary<string, object?> ?? new Dictionary<string, object?>())
            {
                var key = kv.Key?.Trim() ?? "";
                if (key.Length == 0 || fields.ContainsKey(key)) continue;
                fields[key] = kv.Value?.ToString()?.Trim() ?? "";
            }
            result.Add((rowNumber, fields));
        }
        return result;
    }

    private static IEnumerable<(int RowNumber, Dictionary<string, string> Fields)> ReadCsv(string filePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = File.ReadAllText(filePath, DetectEncoding(filePath));
        var lines = SplitCsvLines(text);
        if (lines.Count == 0) yield break;
        var headers = ParseCsvLine(lines[0]);
        for (var i = 1; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = ParseCsvLine(lines[i]);
            var fields = new Dictionary<string, string>();
            for (var c = 0; c < headers.Count; c++)
            {
                var key = headers[c].Trim();
                if (key.Length == 0 || fields.ContainsKey(key)) continue;
                fields[key] = c < values.Count ? values[c].Trim() : "";
            }
            yield return (i + 1, fields);
        }
    }

    private static Encoding DetectEncoding(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) return Encoding.UTF8;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) return Encoding.Unicode;
        // 无 BOM 视为 GB18030（兼容中文 Excel 导出的 ANSI CSV）
        return Encoding.GetEncoding("GB18030");
    }

    private static List<string> SplitCsvLines(string text) =>
        text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else sb.Append(ch);
        }
        result.Add(sb.ToString());
        return result;
    }
}
