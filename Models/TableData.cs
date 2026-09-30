namespace MusicPlatform.WebApi.Models;

public class TableData
{
    public string Name { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];
}
