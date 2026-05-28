namespace BankingApp.Infrastructure.Core.Clock;

using Application.Shared.Clock;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
