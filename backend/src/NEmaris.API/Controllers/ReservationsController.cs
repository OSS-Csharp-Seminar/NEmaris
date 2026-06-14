using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto request)
    {
        try
        {
            var reservedByUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var created = await _reservationService.CreateReservationAsync(request, reservedByUserId);
            return Created($"/api/reservations/{created.Id}", created);
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var reservations = await _reservationService.GetReservationsAsync(fromDate, toDate);
        return Ok(reservations);
    }

    [HttpGet("available-tables")]
    public async Task<IActionResult> GetAvailableTables([FromQuery] GetAvailableTablesQueryDto query)
    {
        try
        {
            var tables = await _reservationService.GetAvailableTablesAsync(query);
            return Ok(tables);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("by-phone")]
    public async Task<IActionResult> GetByPhone([FromQuery] string phone)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByPhoneAsync(phone);
            return Ok(reservations);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateReservationDto request)
    {
        try
        {
            var updated = await _reservationService.UpdateReservationAsync(id, request);
            return Ok(updated);
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
    public async Task<IActionResult> Cancel(long id, [FromQuery] string phone)
    {
        try
        {
            var cancelled = await _reservationService.CancelReservationAsync(id, phone);
            return Ok(cancelled);
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

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> ChangeStatus(long id, [FromBody] ChangeReservationStatusDto request)
    {
        try
        {
            var waiterUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var updated = await _reservationService.ChangeStatusAsync(id, request.Status, waiterUserId);
            return Ok(updated);
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
}
