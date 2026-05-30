using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NEmaris.Domain.Entities;

namespace NEmaris.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RestaurantTables> Tables => Set<RestaurantTables>();
    public DbSet<Guests> Guests => Set<Guests>();
    public DbSet<Reservations> Reservations => Set<Reservations>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Role).HasConversion<int>();
            entity.Property(u => u.Status).HasConversion<int>();
        });

        builder.Entity<RestaurantTables>(entity =>
        {
            entity.ToTable("restaurant_tables");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(t => t.TableNumber).HasColumnName("table_number").HasMaxLength(20).IsRequired();
            entity.Property(t => t.Capacity).HasColumnName("capacity").IsRequired();
            entity.Property(t => t.GuestCount).HasColumnName("guest_count").IsRequired();
            entity.Property(t => t.Zone).HasColumnName("zone").HasMaxLength(100);
            entity.Property(t => t.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            entity.Property(t => t.Floor).HasColumnName("floor").IsRequired();
            entity.Property(t => t.PositionX).HasColumnName("position_x").HasPrecision(5, 2).IsRequired();
            entity.Property(t => t.PositionY).HasColumnName("position_y").HasPrecision(5, 2).IsRequired();
            entity.Property(t => t.Shape).HasColumnName("shape").HasConversion<int>().IsRequired();
            entity.Property(t => t.Rotation).HasColumnName("rotation").IsRequired();
            entity.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(t => t.TableNumber).IsUnique();
            entity.HasIndex(t => t.Floor);
        });

        builder.Entity<MenuCategory>(entity =>
        {
            entity.ToTable("menu_categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(c => c.Description).HasColumnName("description");
            entity.Property(c => c.DisplayOrder).HasColumnName("display_order").IsRequired();
            entity.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasIndex(c => c.DisplayOrder).IsUnique();
        });

        builder.Entity<MenuItem>(entity =>
        {
            entity.ToTable("menu_items");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(m => m.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(m => m.Description).HasColumnName("description");
            entity.Property(m => m.Price).HasColumnName("price").IsRequired();
            entity.Property(m => m.CategoryId).HasColumnName("category_id").IsRequired();

            entity.Property(m => m.Status).HasColumnName("status").IsRequired();
            entity.Property(m => m.IsAvailable).HasColumnName("is_available").IsRequired();
            entity.Property(m => m.Sku).HasColumnName("sku");

            entity.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(m => m.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasOne(m => m.Category)
                  .WithMany()
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Guests>(entity =>
        {
            entity.ToTable("guests");
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(g => g.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            entity.Property(g => g.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            entity.Property(g => g.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
            entity.Property(g => g.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(g => g.Notes).HasColumnName("notes").HasColumnType("text");
            entity.Property(g => g.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(g => g.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(g => g.Phone).IsUnique();
        });

        builder.Entity<Reservations>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(r => r.GuestId).HasColumnName("guest_id").IsRequired();
            entity.Property(r => r.TableId).HasColumnName("table_id").IsRequired();
            entity.Property(r => r.ReservedByUserId).HasColumnName("reserved_by_user_id");
            entity.Property(r => r.ReservationDate).HasColumnName("reservation_date").HasColumnType("date").IsRequired();
            entity.Property(r => r.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(r => r.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(r => r.PartySize).HasColumnName("party_size").IsRequired();
            entity.Property(r => r.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            entity.Property(r => r.SpecialRequest).HasColumnName("special_request").HasColumnType("text");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(r => r.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReservedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReservedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(r => new { r.TableId, r.StartTime, r.EndTime });
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).HasMaxLength(512).IsRequired();
            entity.HasIndex(r => r.Token).IsUnique();
            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(o => o.OrderNumber).HasColumnName("order_number").HasMaxLength(30).IsRequired();
            entity.Property(o => o.TableId).HasColumnName("table_id").IsRequired();
            entity.Property(o => o.WaiterUserId).HasColumnName("waiter_user_id").IsRequired();
            entity.Property(o => o.GuestId).HasColumnName("guest_id");
            entity.Property(o => o.ReservationId).HasColumnName("reservation_id");
            entity.Property(o => o.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            entity.Property(o => o.PaymentStatus).HasColumnName("payment_status").HasConversion<int>().IsRequired();
            entity.Property(o => o.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2).IsRequired();
            entity.Property(o => o.DiscountAmount).HasColumnName("discount_amount").HasPrecision(10, 2).IsRequired();
            entity.Property(o => o.TotalAmount).HasColumnName("total_amount").HasPrecision(10, 2).IsRequired();
            entity.Property(o => o.OpenedAt).HasColumnName("opened_at").IsRequired();
            entity.Property(o => o.ClosedAt).HasColumnName("closed_at");

            entity.HasIndex(o => o.OrderNumber).IsUnique();

            entity.HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Waiter)
                .WithMany()
                .HasForeignKey(o => o.WaiterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.Guest)
                .WithMany()
                .HasForeignKey(o => o.GuestId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.Reservation)
                .WithMany()
                .HasForeignKey(o => o.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(i => i.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(i => i.MenuItemId).HasColumnName("menu_item_id").IsRequired();
            entity.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(i => i.UnitPrice).HasColumnName("unit_price").HasPrecision(10, 2).IsRequired();
            entity.Property(i => i.LineTotal).HasColumnName("line_total").HasPrecision(10, 2).IsRequired();
            entity.Property(i => i.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(i => i.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.MenuItem)
                .WithMany()
                .HasForeignKey(i => i.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(p => p.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(p => p.PaymentMethod).HasColumnName("payment_method").HasConversion<int>().IsRequired();
            entity.Property(p => p.Amount).HasColumnName("amount").HasPrecision(10, 2).IsRequired();
            entity.Property(p => p.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(100).IsRequired();
            entity.Property(p => p.PaidAt).HasColumnName("paid_at").IsRequired();
            entity.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(p => p.ReferenceNumber).IsUnique();

            entity.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
