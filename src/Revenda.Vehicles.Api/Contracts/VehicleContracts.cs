using System.ComponentModel.DataAnnotations;

namespace Revenda.Vehicles.Api.Contracts;

public sealed record SaveVehicleRequest(
    [Required][StringLength(60, MinimumLength = 1)] string Brand,
    [Required][StringLength(60, MinimumLength = 1)] string Model,
    [Range(1900, 2100)] int Year,
    [Required][StringLength(60, MinimumLength = 1)] string Color,
    [Range(0.01, 10_000_000)] decimal Price,
    [Required][StringLength(8, MinimumLength = 7)] string LicensePlate);

public sealed record StartPurchaseRequest([Required] Guid VehicleId);

public sealed record PaymentWebhookRequest(
    [Required][StringLength(40, MinimumLength = 1)] string PaymentCode,
    [Required] string Status);
