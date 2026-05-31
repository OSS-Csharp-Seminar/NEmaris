using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/public/overview")]
[AllowAnonymous]
public class PublicOverviewController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITableService _tableService;

    public PublicOverviewController(AppDbContext db, ITableService tableService)
    {
        _db = db;
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<ActionResult<PublicOverviewDto>> Get()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var tables = await _tableService.GetAllAsync();

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
            TotalTables = tables.Count,
            OccupiedTables = tables.Count(t => t.Status == TableStatus.Seated),
            ReservedTables = tables.Count(t => t.Status == TableStatus.Reserved),
            AvailableTables = tables.Count(t => t.Status == TableStatus.Available),
            ReservationsToday = reservationsToday,
            UpcomingReservations = upcomingReservations
        });
    }
}
