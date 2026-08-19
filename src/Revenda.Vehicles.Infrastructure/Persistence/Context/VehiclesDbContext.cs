using Microsoft.EntityFrameworkCore;
using Revenda.Vehicles.Domain.Entities;

namespace Revenda.Vehicles.Infrastructure.Persistence.Context;

public sealed class VehiclesDbContext : DbContext
{
    public VehiclesDbContext(DbContextOptions<VehiclesDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("vehicles");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehiclesDbContext).Assembly);
    }
}
