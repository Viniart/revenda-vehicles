using FluentAssertions;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_DevePreservarOValorComDuasCasas()
    {
        Money.Create(55_900.50m).Amount.Should().Be(55_900.50m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(55_900.555)]
    public void Create_DeveLancarExcecao_QuandoValorEInvalido(decimal amount)
    {
        var criar = () => Money.Create(amount);

        criar.Should().Throw<InvalidVehicleDataException>();
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoValorExcedeOLimite()
    {
        var criar = () => Money.Create(Money.MaxAmount + 1);

        criar.Should().Throw<InvalidVehicleDataException>();
    }
}

public class LicensePlateTests
{
    [Theory]
    [InlineData("ABC1234", "ABC1234")]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("ABC1D23", "ABC1D23")]
    [InlineData(" abc1d23 ", "ABC1D23")]
    public void Create_DeveNormalizarOsDoisFormatosEmCirculacao(string input, string esperado)
    {
        LicensePlate.Create(input).Value.Should().Be(esperado);
    }

    [Theory]
    [InlineData("AB1234")]
    [InlineData("ABCD123")]
    [InlineData("1234ABC")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_DeveLancarExcecao_QuandoPlacaEInvalida(string? input)
    {
        var criar = () => LicensePlate.Create(input);

        criar.Should().Throw<InvalidVehicleDataException>();
    }
}
