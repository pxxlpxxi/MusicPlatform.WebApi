using Microsoft.AspNetCore.Mvc;
using MusicPlatform.WebApi.Models;
using MusicPlatform.WebApi.Services;

namespace MusicPlatform.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly DatabaseTableService _databaseTableService;

    public TablesController(
        DatabaseTableService databaseTableService)
    {
        _databaseTableService = databaseTableService;
    }

    [HttpGet]
    public ActionResult GetTables()
    {
        return Ok(_databaseTableService.GetTableNames());
    }

    [HttpGet("{name}")]
    public ActionResult GetTable(string name)
    {
        var table = _databaseTableService.GetTable(name);

        if (table == null)
        {
            return NotFound();
        }

        return Ok(table);
    }

    [HttpPost("{name}/rows")]
    public ActionResult InsertRow(
        string name,
        [FromBody] TableRowRequest request)
    {
        try
        {
            int result = _databaseTableService.InsertRow(
                name,
                request.Values);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{name}/rows/{id}")]
    public ActionResult UpdateRow(
        string name,
        string id,
        [FromBody] TableRowRequest request)
    {
        try
        {
            _databaseTableService.UpdateRow(
                name,
                id,
                request.Values);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{name}/rows/{id}")]
    public ActionResult DeleteRow(
        string name,
        string id)
    {
        try
        {
            _databaseTableService.DeleteRow(
                name,
                id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}