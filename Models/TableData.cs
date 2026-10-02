namespace MusicPlatform.WebApi.Models;

public class TableData
{
    public string Name { get; set; } = string.Empty;
    public List<TableColumn> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];
}

public class TableColumn
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsNullable { get; set; }
    public bool IsEditable { get; set; }
}