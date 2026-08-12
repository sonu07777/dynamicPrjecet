using AuditLogService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Data;

public class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserEmail).HasMaxLength(256);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.HasIndex(e => new { e.EntityType, e.Action });
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
