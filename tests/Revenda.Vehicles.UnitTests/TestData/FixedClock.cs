using Revenda.Vehicles.Application.Ports.Output;

namespace Revenda.Vehicles.UnitTests.TestData;

internal sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset instant) => UtcNow = instant;

    public DateTimeOffset UtcNow { get; }
}
