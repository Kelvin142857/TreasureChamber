namespace TreasureChamber.Application.Dtos;

public enum ImportAction
{
    New,
    Update,
    Error
}

public class ImportRow
{
    public int RowNumber { get; set; }
    public string Model { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Series { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public List<KeyValuePair<string, string>> Specs { get; set; } = new();
    public ImportAction Action { get; set; }
    public string? Error { get; set; }
}

public class ImportPreview
{
    public string FileName { get; set; } = "";
    public List<ImportRow> Rows { get; set; } = new();
    public int NewCount => Rows.Count(r => r.Action == ImportAction.New);
    public int UpdateCount => Rows.Count(r => r.Action == ImportAction.Update);
    public int ErrorCount => Rows.Count(r => r.Action == ImportAction.Error);
}

public class ImportResult
{
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}
