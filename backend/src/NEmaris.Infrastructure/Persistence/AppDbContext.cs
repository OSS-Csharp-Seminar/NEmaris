using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NEmaris.Domain.Entities;

namespace NEmaris.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RestaurantTables> Tables => Set<RestaurantTables>();

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
    }
}
