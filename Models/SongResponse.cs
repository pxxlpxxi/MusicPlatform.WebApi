namespace MusicPlatform.WebApi.Models;

public class SongResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string MainArtist { get; set; } = "";
    public List<string> FeaturedArtists { get; set; } = [];
    public List<AlbumResponse> Albums { get; set; } = [];
    public List<MediaResponse> Media { get; set; } = [];
}

public class AlbumResponse
{
    public string Title { get; set; } = "";
    public DateOnly? ReleaseDate { get; set; }
}

public class MediaResponse
{
    public string Type { get; set; } = "";
    public string ExternalId { get; set; } = "";
}
