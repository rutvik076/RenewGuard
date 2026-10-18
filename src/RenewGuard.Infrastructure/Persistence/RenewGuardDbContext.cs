using Microsoft.EntityFrameworkCore;
using RenewGuard.Domain.Models;

namespace RenewGuard.Infrastructure.Persistence;

public class RenewGuardDbContext(DbContextOptions<RenewGuardDbContext> options) : DbContext(options)
{
    public DbSet<ContractRecord> Contracts => Set<ContractRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LastCancelDate);
        });
    }
}
