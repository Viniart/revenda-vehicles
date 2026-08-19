using Microsoft.EntityFrameworkCore;
using Revenda.Vehicles.Infrastructure.Persistence.Context;

namespace Revenda.Vehicles.Infrastructure.Persistence;

public sealed class DatabaseBootstrapper
{
    private readonly VehiclesDbContext _context;

    public DatabaseBootstrapper(VehiclesDbContext context) => _context = context;

    public Task RunAsync(CancellationToken cancellationToken) =>
        _context.Database.MigrateAsync(cancellationToken);
}
