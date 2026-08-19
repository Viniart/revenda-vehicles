using Microsoft.EntityFrameworkCore;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Infrastructure.Persistence.Context;

namespace Revenda.Vehicles.Infrastructure.Persistence.Repositories;

internal sealed class SaleRepository : ISaleRepository
{
    private readonly VehiclesDbContext _context;

    public SaleRepository(VehiclesDbContext context) => _context = context;

    public Task<Sale?> FindByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken) =>
        _context.Sales.FirstOrDefaultAsync(sale => sale.PaymentCode == paymentCode, cancellationToken);

    public async Task<IReadOnlyList<Sale>> ListByBuyerAsync(Guid buyerId, CancellationToken cancellationToken) =>
        await _context.Sales
            .AsNoTracking()
            .Where(sale => sale.BuyerId == buyerId)
            .OrderByDescending(sale => sale.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken) =>
        await _context.Sales.AddAsync(sale, cancellationToken);
}
