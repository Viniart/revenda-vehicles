using FluentAssertions;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.Domain.ValueObjects;
using Revenda.Vehicles.UnitTests.TestData;

namespace Revenda.Vehicles.UnitTests.Domain;

public class VehicleTests
{
    [Fact]
    public void Register_DeveNascerDisponivelComDadosNormalizados()
    {
        var vehicle = Vehicle.Register(
            "  Fiat ",
            " Argo",
            2023,
            "Branco ",
            Money.Create(72_500m),
            LicensePlate.Create("abc-1234"),
            VehicleFactory.Agora);

        vehicle.Brand.Should().Be("Fiat");
        vehicle.Model.Should().Be("Argo");
        vehicle.Color.Should().Be("Branco");
        vehicle.LicensePlate.Value.Should().Be("ABC1234");
        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2028)]
    public void Register_DeveLancarExcecao_QuandoAnoEstaForaDaFaixa(int year)
    {
        var registrar = () => VehicleFactory.Disponivel(year: year);

        registrar.Should().Throw<InvalidVehicleDataException>();
    }

    [Fact]
    public void Register_DeveAceitarModeloDoAnoSeguinte()
    {
        var vehicle = VehicleFactory.Disponivel(year: VehicleFactory.Agora.Year + 1);

        vehicle.Year.Should().Be(VehicleFactory.Agora.Year + 1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_DeveLancarExcecao_QuandoMarcaEstaVazia(string? brand)
    {
        var registrar = () => VehicleFactory.Disponivel(brand: brand!);

        registrar.Should().Throw<InvalidVehicleDataException>();
    }

    [Fact]
    public void Update_DeveAlterarOsDadosEMarcarAAtualizacao()
    {
        var vehicle = VehicleFactory.Disponivel();
        var depois = VehicleFactory.Agora.AddDays(3);

        vehicle.Update(
            "Volkswagen",
            "Gol 1.6",
            2023,
            "Preto",
            Money.Create(59_900m),
            LicensePlate.Create("ABC1D23"),
            depois);

        vehicle.Model.Should().Be("Gol 1.6");
        vehicle.Price.Amount.Should().Be(59_900m);
        vehicle.UpdatedAt.Should().Be(depois);
    }

    [Fact]
    public void Update_DeveLancarExcecao_QuandoVeiculoEstaReservado()
    {
        var vehicle = VehicleFactory.Reservado();

        var editar = () => vehicle.Update(
            "Volkswagen",
            "Gol",
            2022,
            "Azul",
            Money.Create(50_000m),
            LicensePlate.Create("ABC1D23"),
            VehicleFactory.Agora);

        editar.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void Reserve_DeveLancarExcecao_QuandoVeiculoJaEstaReservado()
    {
        var vehicle = VehicleFactory.Reservado();

        var reservar = () => vehicle.Reserve(VehicleFactory.Agora);

        reservar.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void ConfirmSale_DeveLancarExcecao_QuandoVeiculoNaoEstaReservado()
    {
        var vehicle = VehicleFactory.Disponivel();

        var vender = () => vehicle.ConfirmSale(VehicleFactory.Agora);

        vender.Should().Throw<InvalidStateTransitionException>();
    }

    [Fact]
    public void ReleaseReservation_DeveDevolverOVeiculoParaAVitrine()
    {
        var vehicle = VehicleFactory.Reservado();

        vehicle.ReleaseReservation(VehicleFactory.Agora.AddMinutes(30));

        vehicle.Status.Should().Be(VehicleStatus.Available);
    }

    [Fact]
    public void ReleaseReservation_DeveLancarExcecao_QuandoVeiculoJaFoiVendido()
    {
        var vehicle = VehicleFactory.Vendido();

        var liberar = () => vehicle.ReleaseReservation(VehicleFactory.Agora);

        liberar.Should().Throw<InvalidStateTransitionException>();
    }
}
