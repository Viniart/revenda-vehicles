using FluentAssertions;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.UnitTests.TestData;

namespace Revenda.Vehicles.UnitTests.Domain;

public class SaleTests
{
    private static readonly Guid Comprador = Guid.Parse("6f2f2a1e-6d5c-4f9b-9a3e-0b1c2d3e4f50");

    [Fact]
    public void Start_DeveReservarOVeiculoECongelarOPreco()
    {
        var vehicle = VehicleFactory.Disponivel(price: 55_900m);

        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);

        vehicle.Status.Should().Be(VehicleStatus.Reserved);
        sale.Status.Should().Be(SaleStatus.PendingPayment);
        sale.Price.Amount.Should().Be(55_900m);
        sale.BuyerId.Should().Be(Comprador);
        sale.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Start_DeveLancarExcecao_QuandoVeiculoJaEstaVendido()
    {
        var vehicle = VehicleFactory.Vendido();

        var comprar = () => Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);

        comprar.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Start_DeveLancarExcecao_QuandoCompradorNaoEIdentificado()
    {
        var vehicle = VehicleFactory.Disponivel();

        var comprar = () => Sale.Start(vehicle, Guid.Empty, "PAY-001", VehicleFactory.Agora);

        comprar.Should().Throw<InvalidVehicleDataException>();
        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Fact]
    public void ConfirmPayment_DeveEfetivarAVendaEMarcarOVeiculoComoVendido()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);
        var pagamento = VehicleFactory.Agora.AddMinutes(15);

        sale.ConfirmPayment(vehicle, pagamento);

        sale.Status.Should().Be(SaleStatus.Paid);
        sale.CompletedAt.Should().Be(pagamento);
        vehicle.Status.Should().Be(VehicleStatus.Sold);
    }

    [Fact]
    public void Cancel_DeveDevolverOVeiculoParaAVitrine()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);

        sale.Cancel(vehicle, VehicleFactory.Agora.AddMinutes(5));

        sale.Status.Should().Be(SaleStatus.Cancelled);
        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Fact]
    public void ConfirmPayment_DeveLancarExcecao_QuandoAVendaJaFoiPaga()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);
        sale.ConfirmPayment(vehicle, VehicleFactory.Agora);

        var reconfirmar = () => sale.ConfirmPayment(vehicle, VehicleFactory.Agora);

        reconfirmar.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Cancel_DeveLancarExcecao_QuandoAVendaJaFoiCancelada()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);
        sale.Cancel(vehicle, VehicleFactory.Agora);

        var cancelarDeNovo = () => sale.Cancel(vehicle, VehicleFactory.Agora);

        cancelarDeNovo.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void ConfirmPayment_DeveLancarExcecao_QuandoOVeiculoNaoPertenceAVenda()
    {
        var vehicle = VehicleFactory.Disponivel();
        var sale = Sale.Start(vehicle, Comprador, "PAY-001", VehicleFactory.Agora);
        var outro = VehicleFactory.Disponivel(plate: "XYZ9A88");
        outro.Reserve(VehicleFactory.Agora);

        var confirmar = () => sale.ConfirmPayment(outro, VehicleFactory.Agora);

        confirmar.Should().Throw<InvalidStateTransitionException>();
    }
}
