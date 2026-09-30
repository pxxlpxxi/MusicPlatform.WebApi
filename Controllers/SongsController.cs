using Microsoft.AspNetCore.Mvc;
using MusicPlatform.Application.Models;
using MusicPlatform.Application.Services;
using MusicPlatform.WebApi.Mappers;
using MusicPlatform.WebApi.Models;

namespace MusicPlatform.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly SongCreationApplicationService _songCreationApplicationService;
    private readonly SongApplicationService _songApplicationService;

    public SongsController(
        SongCreationApplicationService songCreationApplicationService,
        SongApplicationService songApplicationService)
    {
        _songCreationApplicationService = songCreationApplicationService;
        _songApplicationService = songApplicationService;
    }

    [HttpGet]
    public ActionResult GetSongs()// https://localhost:7118/api/songs
    {
        List<SongInfo> songs = _songApplicationService.GetSongs();

        return Ok(
            songs.Select(SongResponseMapper.ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public ActionResult GetSong(int id) //https://localhost:7118/api/songs/{id}
    {
        try
        {
            SongInfo song = _songApplicationService.GetSong(id);

            return Ok(SongResponseMapper.ToResponse(song));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("search")]
    public ActionResult SearchSongs(string term)  //https://localhost:7118/api/songs/search?term=BR%C3%86NDER
    {
        try
        {
            List<SongInfo> songs =
                _songApplicationService.SearchSongs(term);

            return Ok(
                songs.Select(SongResponseMapper.ToResponse).ToList());
        }
        catch (ArgumentException)
        {
            return BadRequest("Search term cannot be empty.");
        }
    }


    [HttpPost]
    public ActionResult CreateSong(SongRequest request)
    {
        try
        {
            SongInfo songInfo =
                SongRequestMapper.ToApplicationModel(request);

            SongInfo createdSong =
                _songCreationApplicationService.CreateSong(songInfo);

            return CreatedAtAction(
                nameof(GetSong),
                new { id = createdSong.Id },
                SongResponseMapper.ToResponse(createdSong));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteSong(int id)
    {
        try
        {
            _songApplicationService.DeleteSong(id);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    public ActionResult UpdateSongTitle(int id, [FromBody] string newTitle)
    {
        try
        {
            _songApplicationService.UpdateSongTitle(id, newTitle);

            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

}

