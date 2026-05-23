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

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(OrderStatus? status = null)
    {
        var query = _db.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        return await query
            .OrderByDescending(o => o.OpenedAt)
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

        // Merge with existing line if same item already on this order
        var existing = await _db.OrderItems
            .Include(i => i.MenuItem)
            .FirstOrDefaultAsync(i => i.OrderId == item.OrderId && i.MenuItemId == item.MenuItemId);

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

    public async Task UpdateOrderItemAsync(OrderItem item)
    {
        _db.OrderItems.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveOrderItemAsync(OrderItem item)
    {
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
        table.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
