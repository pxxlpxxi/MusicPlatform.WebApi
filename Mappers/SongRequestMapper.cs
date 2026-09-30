using MusicPlatform.Application.Models;
using MusicPlatform.WebApi.Models;

namespace MusicPlatform.WebApi.Mappers;

public static class SongRequestMapper
{
    public static SongInfo ToApplicationModel(SongRequest request)
    {
        return new SongInfo
        {
            Title = request.Title,
            MainArtist = request.MainArtist,
            FeaturedArtists = request.FeaturedArtists.ToList(),

            Albums = request.Albums
                .Select(a => new AlbumInfo
                {
                    Title = a.Title,
                    ReleaseDate = a.ReleaseDate
                })
                .ToList(),

            Media = request.Media
                .Select(m => new MediaInfo
                {
                    Type = m.Type,
                    ExternalId = m.ExternalId
                })
                .ToList()
        };
    }
}
