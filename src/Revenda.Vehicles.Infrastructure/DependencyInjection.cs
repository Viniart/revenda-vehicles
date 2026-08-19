using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Infrastructure.Payments;
using Revenda.Vehicles.Infrastructure.Persistence;
using Revenda.Vehicles.Infrastructure.Persistence.Context;
using Revenda.Vehicles.Infrastructure.Persistence.Repositories;
using Revenda.Vehicles.Infrastructure.Time;

namespace Revenda.Vehicles.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVehiclesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VehiclesDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__migrations", "vehicles")));

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DatabaseBootstrapper>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPaymentCodeGenerator, RandomPaymentCodeGenerator>();

        return services;
    }
}
