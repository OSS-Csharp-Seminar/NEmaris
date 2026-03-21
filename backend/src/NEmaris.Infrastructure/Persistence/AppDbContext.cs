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
            entity.Property(t => t.Zone).HasColumnName("zone").HasMaxLength(100);
            entity.Property(t => t.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            entity.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();

            entity.HasIndex(t => t.TableNumber).IsUnique();
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
    }
}
