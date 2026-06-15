using Microsoft.EntityFrameworkCore;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure.Persistence;

namespace NEmaris.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Order?> GetByIdAsync(long id)
        => await _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetBillAsync(long id)
        => await _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetOpenOrderByTableIdAsync(long tableId)
        => await _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.TableId == tableId && o.Status == OrderStatus.Open);

    public Task<bool> HasOpenOrderForReservationAsync(long reservationId)
        => _db.Orders.AnyAsync(o => o.ReservationId == reservationId && o.Status == OrderStatus.Open);

    public async Task<RestaurantTables?> GetTableByIdAsync(long tableId)
        => await _db.Tables.FindAsync(tableId);

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(OrderStatus? status = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (from.HasValue)
            query = query.Where(o => o.OpenedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.OpenedAt < to.Value);

        return await query
            .OrderByDescending(o => o.OpenedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersForStatsAsync(DateTime from, DateTime to)
    {
        return await _db.Orders
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Payments)
            .Where(o => o.OpenedAt >= from && o.OpenedAt < to)
            .Where(o => o.Status == OrderStatus.Closed)
            .ToListAsync();
    }

    public async Task<Order> AddOrderAsync(Order order)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return await _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == order.Id);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }

    public async Task<OrderItem> AddOrderItemAsync(OrderItem item)
    {
        var menuItem = await _db.MenuItems.FindAsync(item.MenuItemId)
            ?? throw new KeyNotFoundException($"Menu item {item.MenuItemId} not found.");

        if (!menuItem.IsAvailable)
            throw new InvalidOperationException($"'{menuItem.Name}' is not available.");

        if (menuItem.StockQuantity < item.Quantity)
            throw new InvalidOperationException($"Only {menuItem.StockQuantity} units of '{menuItem.Name}' are available.");

        // Merge with existing line if same item already on this order
        var existing = await _db.OrderItems
            .Include(i => i.MenuItem)
            .FirstOrDefaultAsync(i => i.OrderId == item.OrderId && i.MenuItemId == item.MenuItemId);

        menuItem.StockQuantity -= item.Quantity;
        menuItem.UpdatedAt = DateTime.UtcNow;

        if (existing is not null)
        {
            existing.Quantity += item.Quantity;
            existing.LineTotal = existing.UnitPrice * existing.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return existing;
        }

        item.UnitPrice = menuItem.Price;
        item.LineTotal = menuItem.Price * item.Quantity;
        item.MenuItem = menuItem;

        _db.OrderItems.Add(item);
        await _db.SaveChangesAsync();

        return item;
    }

    public async Task<OrderItem?> GetOrderItemByIdAsync(long itemId)
        => await _db.OrderItems
            .Include(i => i.MenuItem)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task UpdateOrderItemAsync(OrderItem item, int previousQuantity)
    {
        var menuItem = await _db.MenuItems.FindAsync(item.MenuItemId)
            ?? throw new KeyNotFoundException($"Menu item {item.MenuItemId} not found.");

        var quantityDelta = item.Quantity - previousQuantity;
        if (quantityDelta > 0 && menuItem.StockQuantity < quantityDelta)
            throw new InvalidOperationException($"Only {menuItem.StockQuantity} additional units of '{menuItem.Name}' are available.");

        menuItem.StockQuantity -= quantityDelta;
        menuItem.UpdatedAt = DateTime.UtcNow;

        _db.OrderItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveOrderItemAsync(OrderItem item)
    {
        var menuItem = await _db.MenuItems.FindAsync(item.MenuItemId);
        if (menuItem is not null)
        {
            menuItem.StockQuantity += item.Quantity;
            menuItem.UpdatedAt = DateTime.UtcNow;
        }

        _db.OrderItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task UpdateTableStatusAsync(long tableId, TableStatus status)
    {
        var table = await _db.Tables.FindAsync(tableId);
        if (table is null) return;

        table.Status = status;
        if (status == TableStatus.Available)
            table.GuestCount = 0;
        table.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
