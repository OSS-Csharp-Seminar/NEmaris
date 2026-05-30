using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using System.Security.Claims;

namespace NEmaris.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var waiterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user ID not found.");

        try
        {
            var order = await _service.CreateOrderAsync(dto, waiterUserId);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] bool todayOnly = true)
    {
        var orders = await _service.GetOrdersAsync(status, todayOnly);
        return Ok(orders);
    }

    [HttpGet("stats/today")]
    public async Task<IActionResult> GetTodayStats()
    {
        var stats = await _service.GetTodayStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOrder(long id)
    {
        var order = await _service.GetOrderAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("by-table/{tableId:long}")]
    public async Task<IActionResult> GetOpenOrderByTable(long tableId)
    {
        var order = await _service.GetOpenOrderByTableIdAsync(tableId);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("{id:long}/bill")]
    public async Task<IActionResult> GetBill(long id)
    {
        try
        {
            var bill = await _service.GetBillAsync(id);
            return Ok(bill);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpPost("{id:long}/items")]
    public async Task<IActionResult> AddItem(long id, [FromBody] AddOrderItemDto dto)
    {
        try
        {
            var item = await _service.AddOrderItemAsync(id, dto);
            return Ok(item);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPut("{id:long}/items/{itemId:long}")]
    public async Task<IActionResult> UpdateItem(long id, long itemId, [FromBody] UpdateOrderItemDto dto)
    {
        try
        {
            var item = await _service.UpdateOrderItemAsync(id, itemId, dto);
            return Ok(item);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpDelete("{id:long}/items/{itemId:long}")]
    public async Task<IActionResult> RemoveItem(long id, long itemId)
    {
        try
        {
            await _service.RemoveOrderItemAsync(id, itemId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("{id:long}/pay")]
    public async Task<IActionResult> ProcessPayment(long id, [FromBody] CreatePaymentDto dto)
    {
        try
        {
            var bill = await _service.ProcessPaymentAsync(id, dto);
            return Ok(bill);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> CancelOrder(long id)
    {
        try
        {
            var order = await _service.CancelOrderAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }
}
