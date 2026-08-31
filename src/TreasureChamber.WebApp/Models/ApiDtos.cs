namespace TreasureChamber.WebApp.Models;

// ================= 产品与目录 =================
public record ProductCardDto(int Id, string Model, string Name, string? SeriesName, string? CategoryName, string? ImagePath);
public record ImageDto(int Id, string Path, int SortOrder);
public record SpecDto(int Id, string Name, string Value);
public record ProductDetailDto(int Id, string Model, string Name, string? Description,
    int? SeriesId, string? SeriesName, int? CategoryId, string? CategoryName, bool IsActive, DateTime UpdatedAt,
    List<ImageDto> Images, List<SpecDto> Specs);
public record ProductPickerDto(int Id, string Model, string Name);
public record CategoryNodeDto(int Id, string Name, int Count, List<CategoryNodeDto> Children);
public record SeriesDto(int Id, string Name, int Count);
public record CatalogDto(List<SeriesDto> Series, List<CategoryNodeDto> CategoryTree, List<ProductCardDto> Recent, int ProductCount);
public record PagedDto<T>(List<T> Items, int Total, int Page, int PageSize, int TotalPages);

public record ProductEditDto(int? Id, string Model, string Name, string? Description,
    int? SeriesId, int? CategoryId, bool IsActive, List<string>? SpecNames, List<string>? SpecValues);

// ================= 意向单 =================
public record IntentOrderItemDto(int? ProductId, string? Model, int Quantity, string? Remark);
public record IntentOrderCreateDto(string CustomerName, string CustomerPhone, string? CustomerCompany,
    string? Note, List<IntentOrderItemDto>? Items);
public record IntentOrderItemViewDto(int Id, int? ProductId, string ProductModel, string ProductName, int Quantity, string? Remark);
public record IntentOrderDto(int Id, string OrderNo, string CustomerName, string CustomerPhone,
    string? CustomerCompany, string? Note, int Status, string StatusLabel, DateTime CreatedAt,
    List<IntentOrderItemViewDto> Items);

// ================= 导入 =================
public record ImportRowDto(int RowNumber, string Model, string Name, string? Series, string? Category,
    string Action, string? Error);
public record ImportPreviewDto(string FileName, List<ImportRowDto> Rows,
    int NewCount, int UpdateCount, int ErrorCount, bool Truncated);
public record ImportResultDto(int Added, int Updated, int Skipped, List<string> Errors);
public record ImportImagesResultDto(int Uploaded, List<ImportImagesItemDto> Matched, List<string> Unmatched);
public record ImportImagesItemDto(string Model, string FileName);

// ================= 二维码 / 设置 =================
public record QrLabelDto(int Id, string Model, string Name);
public record SettingsDto(string BaseUrl, string RequestBase, List<string> LanIps);
public record SettingsUpdateDto(string BaseUrl);
