using Microsoft.EntityFrameworkCore;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.ValueObjects;
using Revenda.Vehicles.Infrastructure.Persistence.Context;

namespace Revenda.Vehicles.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository : IVehicleRepository
{
    private readonly VehiclesDbContext _context;

    public VehicleRepository(VehiclesDbContext context) => _context = context;

    public Task<Vehicle?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> ListByStatusAsync(
        VehicleStatus status,
        CancellationToken cancellationToken) =>
        await _context.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.Status == status)
            .OrderBy(vehicle => vehicle.Price)
            .ThenBy(vehicle => vehicle.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByLicensePlateAsync(
        LicensePlate licensePlate,
        Guid? ignoredVehicleId,
        CancellationToken cancellationToken) =>
        _context.Vehicles.AnyAsync(
            vehicle => vehicle.LicensePlate == licensePlate
                && (ignoredVehicleId == null || vehicle.Id != ignoredVehicleId),
            cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken) =>
        await _context.Vehicles.AddAsync(vehicle, cancellationToken);
}
