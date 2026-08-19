using FluentAssertions;
using NSubstitute;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Application.UseCases.Sales;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.UnitTests.TestData;

namespace Revenda.Vehicles.UnitTests.Application;

public class StartPurchaseUseCaseTests
{
    private static readonly Guid Comprador = Guid.Parse("6f2f2a1e-6d5c-4f9b-9a3e-0b1c2d3e4f50");

    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly ISaleRepository _sales = Substitute.For<ISaleRepository>();
    private readonly IPaymentCodeGenerator _paymentCodes = Substitute.For<IPaymentCodeGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StartPurchaseUseCase _useCase;

    public StartPurchaseUseCaseTests()
    {
        _paymentCodes.Generate().Returns("PAY-0001");
        _useCase = new StartPurchaseUseCase(
            _vehicles,
            _sales,
            _paymentCodes,
            _unitOfWork,
            new FixedClock(VehicleFactory.Agora));
    }

    [Fact]
    public async Task ExecuteAsync_DeveReservarOVeiculoEAbrirAVendaPendente()
    {
        var vehicle = VehicleFactory.Disponivel(price: 48_000m);
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var output = await _useCase.ExecuteAsync(
            new StartPurchaseInput(vehicle.Id, Comprador),
            CancellationToken.None);

        output.Status.Should().Be(nameof(SaleStatus.PendingPayment));
        output.PaymentCode.Should().Be("PAY-0001");
        output.Price.Should().Be(48_000m);
        vehicle.Status.Should().Be(VehicleStatus.Reserved);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarExcecao_QuandoVeiculoNaoExiste()
    {
        _vehicles.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var executar = () => _useCase.ExecuteAsync(
            new StartPurchaseInput(Guid.NewGuid(), Comprador),
            CancellationToken.None);

        await executar.Should().ThrowAsync<VehicleNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_NaoDeveGravar_QuandoOVeiculoJaEstaReservado()
    {
        var vehicle = VehicleFactory.Reservado();
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var executar = () => _useCase.ExecuteAsync(
            new StartPurchaseInput(vehicle.Id, Comprador),
            CancellationToken.None);

        await executar.Should().ThrowAsync<Revenda.Vehicles.Domain.Exceptions.InvalidStateTransitionException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }
}

public class ConfirmPaymentUseCaseTests
{
    private static readonly Guid Comprador = Guid.Parse("6f2f2a1e-6d5c-4f9b-9a3e-0b1c2d3e4f50");

    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly ISaleRepository _sales = Substitute.For<ISaleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConfirmPaymentUseCase _useCase;

    public ConfirmPaymentUseCaseTests() =>
        _useCase = new ConfirmPaymentUseCase(
            _sales,
            _vehicles,
            _unitOfWork,
            new FixedClock(VehicleFactory.Agora.AddMinutes(10)));

    [Fact]
    public async Task ExecuteAsync_DeveEfetivarAVendaEMarcarOVeiculoComoVendido()
    {
        var (vehicle, sale) = Cenario();

        var output = await _useCase.ExecuteAsync(
            new ConfirmPaymentInput("PAY-0001", Approved: true),
            CancellationToken.None);

        output.Status.Should().Be(nameof(SaleStatus.Paid));
        output.CompletedAt.Should().Be(VehicleFactory.Agora.AddMinutes(10));
        vehicle.Status.Should().Be(VehicleStatus.Sold);
        sale.Status.Should().Be(SaleStatus.Paid);
    }

    [Fact]
    public async Task ExecuteAsync_DeveDevolverOVeiculoParaAVitrine_QuandoOPagamentoERecusado()
    {
        var (vehicle, _) = Cenario();

        var output = await _useCase.ExecuteAsync(
            new ConfirmPaymentInput("PAY-0001", Approved: false),
            CancellationToken.None);

        output.Status.Should().Be(nameof(SaleStatus.Cancelled));
        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Fact]
    public async Task ExecuteAsync_DeveSerIdempotente_QuandoOGatewayReenviaANotificacao()
    {
        var (_, sale) = Cenario();
        await _useCase.ExecuteAsync(new ConfirmPaymentInput("PAY-0001", true), CancellationToken.None);
        _unitOfWork.ClearReceivedCalls();

        var output = await _useCase.ExecuteAsync(
            new ConfirmPaymentInput("PAY-0001", true),
            CancellationToken.None);

        output.Status.Should().Be(nameof(SaleStatus.Paid));
        sale.CompletedAt.Should().Be(VehicleFactory.Agora.AddMinutes(10));
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarExcecao_QuandoOCodigoDePagamentoNaoExiste()
    {
        _sales.FindByPaymentCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var executar = () => _useCase.ExecuteAsync(
            new ConfirmPaymentInput("PAY-9999", true),
            CancellationToken.None);

        await executar.Should().ThrowAsync<SaleNotFoundException>();
    }

    private (Vehicle Vehicle, Sale Sale) Cenario()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-0001", VehicleFactory.Agora);

        _sales.FindByPaymentCodeAsync("PAY-0001", Arg.Any<CancellationToken>()).Returns(sale);
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        return (vehicle, sale);
    }
}
