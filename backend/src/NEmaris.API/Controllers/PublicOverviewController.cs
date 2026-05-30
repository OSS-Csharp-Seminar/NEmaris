using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEmaris.Application.DTOs;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/public/overview")]
[AllowAnonymous]
public class PublicOverviewController : ControllerBase
{
    private readonly AppDbContext _db;

    public PublicOverviewController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PublicOverviewDto>> Get()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var tableCounts = await _db.Tables
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(tables => new
            {
                Total = tables.Count(),
                Occupied = tables.Count(table => table.Status == TableStatus.Seated),
                Reserved = tables.Count(table => table.Status == TableStatus.Reserved),
                Available = tables.Count(table => table.Status == TableStatus.Available)
            })
            .FirstOrDefaultAsync();

        var reservationsToday = await _db.Reservations
            .AsNoTracking()
            .CountAsync(reservation =>
                reservation.ReservationDate == today &&
                reservation.Status == ReservationStatus.Active);

        var upcomingReservations = await _db.Reservations
            .AsNoTracking()
            .CountAsync(reservation =>
                reservation.ReservationDate > today &&
                reservation.Status == ReservationStatus.Active);

        return Ok(new PublicOverviewDto
        {
            TotalTables = tableCounts?.Total ?? 0,
            OccupiedTables = tableCounts?.Occupied ?? 0,
            ReservedTables = tableCounts?.Reserved ?? 0,
            AvailableTables = tableCounts?.Available ?? 0,
            ReservationsToday = reservationsToday,
            UpcomingReservations = upcomingReservations
        });
    }
}
