using Microsoft.Extensions.DependencyInjection;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.UseCases.Sales;
using Revenda.Vehicles.Application.UseCases.Vehicles;

namespace Revenda.Vehicles.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVehiclesApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegisterVehicleUseCase, RegisterVehicleUseCase>();
        services.AddScoped<IUpdateVehicleUseCase, UpdateVehicleUseCase>();
        services.AddScoped<IGetVehicleUseCase, GetVehicleUseCase>();
        services.AddScoped<IListVehiclesUseCase, ListVehiclesUseCase>();
        services.AddScoped<IStartPurchaseUseCase, StartPurchaseUseCase>();
        services.AddScoped<IConfirmPaymentUseCase, ConfirmPaymentUseCase>();
        services.AddScoped<IListBuyerSalesUseCase, ListBuyerSalesUseCase>();

        return services;
    }
}
