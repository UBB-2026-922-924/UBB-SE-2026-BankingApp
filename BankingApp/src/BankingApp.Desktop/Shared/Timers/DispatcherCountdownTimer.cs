namespace BankingApp.Desktop.Shared.Timers;

using System;
using Microsoft.UI.Xaml;

/// <summary>
///     Production implementation of <see cref="ICountdownTimer"/> backed by <see cref="DispatcherTimer"/>.
///     Events are always raised on the UI thread, making it safe for ViewModels to update observable
///     properties from the handler without extra marshaling.
/// </summary>
public sealed class DispatcherCountdownTimer : ICountdownTimer
{
    private const int TimerIntervalSeconds = 1;
    private readonly DispatcherTimer _inner;

    /// <summary>Initializes a new instance of the <see cref="DispatcherCountdownTimer"/> class with a one-second interval.</summary>
    public DispatcherCountdownTimer()
    {
        _inner = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TimerIntervalSeconds) };
        _inner.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public event EventHandler? Tick;

    /// <inheritdoc />
    public void Start() => _inner.Start();

    /// <inheritdoc />
    public void StopTimer() => _inner.Stop();
}
