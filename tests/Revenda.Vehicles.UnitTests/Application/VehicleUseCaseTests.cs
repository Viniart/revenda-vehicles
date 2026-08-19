using FluentAssertions;
using NSubstitute;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Application.UseCases.Vehicles;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.Domain.ValueObjects;
using Revenda.Vehicles.UnitTests.TestData;

namespace Revenda.Vehicles.UnitTests.Application;

public class RegisterVehicleUseCaseTests
{
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterVehicleUseCase _useCase;

    public RegisterVehicleUseCaseTests() =>
        _useCase = new RegisterVehicleUseCase(_vehicles, _unitOfWork, new FixedClock(VehicleFactory.Agora));

    [Fact]
    public async Task ExecuteAsync_DevePersistirOVeiculoComoDisponivel()
    {
        Vehicle? persistido = null;
        await _vehicles.AddAsync(Arg.Do<Vehicle>(v => persistido = v), Arg.Any<CancellationToken>());

        var output = await _useCase.ExecuteAsync(EntradaValida(), CancellationToken.None);

        persistido!.Status.Should().Be(VehicleStatus.Available);
        output.LicensePlate.Should().Be("ABC1D23");
        output.Price.Should().Be(55_900m);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarExcecao_QuandoPlacaJaExiste()
    {
        _vehicles
            .ExistsByLicensePlateAsync(Arg.Any<LicensePlate>(), null, Arg.Any<CancellationToken>())
            .Returns(true);

        var executar = () => _useCase.ExecuteAsync(EntradaValida(), CancellationToken.None);

        await executar.Should().ThrowAsync<DuplicateLicensePlateException>();
        await _vehicles.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task ExecuteAsync_NaoDeveGravar_QuandoPrecoEInvalido()
    {
        var executar = () => _useCase.ExecuteAsync(EntradaValida() with { Price = 0 }, CancellationToken.None);

        await executar.Should().ThrowAsync<InvalidVehicleDataException>();
        await _unitOfWork.DidNotReceiveWithAnyArgs().CommitAsync(default);
    }

    private static RegisterVehicleInput EntradaValida() =>
        new("Volkswagen", "Gol", 2022, "Prata", 55_900m, "abc1d23");
}

public class UpdateVehicleUseCaseTests
{
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateVehicleUseCase _useCase;

    public UpdateVehicleUseCaseTests() =>
        _useCase = new UpdateVehicleUseCase(_vehicles, _unitOfWork, new FixedClock(VehicleFactory.Agora));

    [Fact]
    public async Task ExecuteAsync_DeveAtualizarOAnuncio()
    {
        var vehicle = VehicleFactory.Disponivel();
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var output = await _useCase.ExecuteAsync(
            new UpdateVehicleInput(vehicle.Id, "Volkswagen", "Gol 1.6", 2023, "Preto", 59_900m, "ABC1D23"),
            CancellationToken.None);

        output.Model.Should().Be("Gol 1.6");
        output.Price.Should().Be(59_900m);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarExcecao_QuandoVeiculoNaoExiste()
    {
        _vehicles.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Vehicle?)null);

        var executar = () => _useCase.ExecuteAsync(
            new UpdateVehicleInput(Guid.NewGuid(), "Fiat", "Argo", 2023, "Branco", 70_000m, "ABC1D23"),
            CancellationToken.None);

        await executar.Should().ThrowAsync<VehicleNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_DeveIgnorarOProprioVeiculoAoChecarPlacaDuplicada()
    {
        var vehicle = VehicleFactory.Disponivel();
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        await _useCase.ExecuteAsync(
            new UpdateVehicleInput(vehicle.Id, "Volkswagen", "Gol", 2022, "Prata", 55_900m, "ABC1D23"),
            CancellationToken.None);

        await _vehicles.Received(1).ExistsByLicensePlateAsync(
            Arg.Any<LicensePlate>(),
            vehicle.Id,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeveLancarExcecao_QuandoVeiculoEstaReservado()
    {
        var vehicle = VehicleFactory.Reservado();
        _vehicles.FindByIdAsync(vehicle.Id, Arg.Any<CancellationToken>()).Returns(vehicle);

        var executar = () => _useCase.ExecuteAsync(
            new UpdateVehicleInput(vehicle.Id, "Volkswagen", "Gol", 2022, "Prata", 50_000m, "ABC1D23"),
            CancellationToken.None);

        await executar.Should().ThrowAsync<InvalidStateTransitionException>();
    }
}

public class ListVehiclesUseCaseTests
{
    private readonly IVehicleRepository _vehicles = Substitute.For<IVehicleRepository>();

    [Fact]
    public async Task ExecuteAsync_DeveConsultarDisponiveis_QuandoOFiltroEAVitrine()
    {
        _vehicles
            .ListByStatusAsync(VehicleStatus.Available, Arg.Any<CancellationToken>())
            .Returns([VehicleFactory.Disponivel()]);

        var resultado = await new ListVehiclesUseCase(_vehicles)
            .ExecuteAsync(VehicleListFilter.ForSale, CancellationToken.None);

        resultado.Should().ContainSingle();
        await _vehicles.Received(1).ListByStatusAsync(VehicleStatus.Available, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeveConsultarVendidos_QuandoOFiltroEVendidos()
    {
        await new ListVehiclesUseCase(_vehicles).ExecuteAsync(VehicleListFilter.Sold, CancellationToken.None);

        await _vehicles.Received(1).ListByStatusAsync(VehicleStatus.Sold, Arg.Any<CancellationToken>());
    }
}
