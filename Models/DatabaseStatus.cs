namespace MusicPlatform.WebApi.Models;

public class DatabaseStatus
{
    public int TableCount { get; set; }

    public List<TableStatus> Tables { get; set; } = [];
}

public class TableStatus
{
    public string Name { get; set; } = string.Empty;

    public long RowCount { get; set; }
}
