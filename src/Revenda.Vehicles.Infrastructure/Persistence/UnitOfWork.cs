using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Infrastructure.Persistence.Context;

namespace Revenda.Vehicles.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly VehiclesDbContext _context;

    public UnitOfWork(VehiclesDbContext context) => _context = context;

    public Task CommitAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
