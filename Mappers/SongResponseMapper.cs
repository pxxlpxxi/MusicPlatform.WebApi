using MusicPlatform.Application.Models;
using MusicPlatform.WebApi.Models;

namespace MusicPlatform.WebApi.Mappers;

public static class SongResponseMapper
{
    public static SongResponse ToResponse(SongInfo song)
    {
        return new SongResponse
        {
            Id = song.Id,
            Title = song.Title,
            MainArtist = song.MainArtist,
            FeaturedArtists = song.FeaturedArtists.ToList(),

            Albums = song.Albums
                .Select(a => new AlbumResponse
                {
                    Title = a.Title,
                    ReleaseDate = a.ReleaseDate
                })
                .ToList(),

            Media = song.Media
                .Select(m => new MediaResponse
                {
                    Type = m.Type,
                    ExternalId = m.ExternalId
                })
                .ToList()
        };
    }
}
