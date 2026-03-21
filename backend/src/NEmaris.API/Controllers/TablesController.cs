using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/tables")]
[Authorize]
public class TablesController : ControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tables = await _tableService.GetAllAsync();
        return Ok(tables);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var table = await _tableService.GetByIdAsync(id);
            return Ok(table);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTableDto request)
    {
        try
        {
            var tableId = await _tableService.CreateTableAsync(request);
            return Created($"/api/tables/{tableId}", new { id = tableId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateTableDto request)
    {
        try
        {
            await _tableService.UpdateTableAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _tableService.DeleteTableAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
