using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        return Ok(tables.Select(ToResponse));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var table = await _tableService.GetByIdAsync(id);
            return Ok(ToResponse(table));
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

    [HttpPatch("{id:long}/guest-count")]
    public async Task<IActionResult> ChangeGuestCount(long id, [FromBody] UpdateTableGuestCountDto request)
    {
        try
        {
            return Ok(ToResponse(await _tableService.ChangeGuestCountAsync(id, request.Change)));
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

    [HttpPost("{id:long}/occupy")]
    public async Task<IActionResult> MarkOccupied(long id)
    {
        try
        {
            var waiterUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                throw new InvalidOperationException("Authenticated user ID not found.");

            return Ok(ToResponse(await _tableService.MarkOccupiedAsync(id, waiterUserId)));
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

    [HttpPost("{id:long}/walkin")]
    public async Task<IActionResult> SeatWalkIn(long id, [FromBody] SeatWalkInDto request)
    {
        try
        {
            var waiterUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                throw new InvalidOperationException("Authenticated user ID not found.");

            return Ok(ToResponse(await _tableService.SeatWalkInAsync(id, request.GuestCount, waiterUserId)));
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

    private static object ToResponse(TableDto table)
    {
        return new
        {
            table.Id,
            table.TableNumber,
            table.Capacity,
            table.GuestCount,
            table.Zone,
            Status = (int)table.Status,
            table.Floor,
            table.PositionX,
            table.PositionY,
            Shape = (int)table.Shape,
            table.Rotation,
            table.CreatedAt,
            table.UpdatedAt,
            table.UpcomingReservationAt
        };
    }
}
