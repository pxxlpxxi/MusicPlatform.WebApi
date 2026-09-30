using Microsoft.AspNetCore.Mvc;
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
}
