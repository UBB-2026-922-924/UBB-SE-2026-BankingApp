namespace BankingApp.Application.Shared.Clock;

/// <summary>
///     Abstracts the system clock so that time-dependent logic can be tested deterministically.
/// </summary>
public interface ISystemClock
{
    /// <summary>Gets the current UTC date and time.</summary>
    public DateTime UtcNow { get; }
}