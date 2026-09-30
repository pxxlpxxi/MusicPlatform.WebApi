namespace MusicPlatform.WebApi.Models;

public class SongRequest
{
    public string Title { get; set; } = "";
    public string MainArtist { get; set; } = "";
    public List<string> FeaturedArtists { get; set; } = [];
    public List<AlbumRequest> Albums { get; set; } = [];
    public List<MediaRequest> Media { get; set; } = [];
}

public class AlbumRequest
{
    public string Title { get; set; } = "";
    public DateOnly? ReleaseDate { get; set; }
}

public class MediaRequest
{
    public string Type { get; set; } = "";
    public string ExternalId { get; set; } = "";
}
