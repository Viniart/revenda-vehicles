using Revenda.Vehicles.Application.Ports.Output;

namespace Revenda.Vehicles.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
