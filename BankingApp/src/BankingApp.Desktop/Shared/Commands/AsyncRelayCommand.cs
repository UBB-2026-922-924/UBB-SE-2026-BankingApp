namespace BankingApp.Desktop.Shared.Commands;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

/// <summary>
///     A command that executes an asynchronous action and prevents concurrent execution.
/// </summary>
public partial class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _executeAsyncAction;
    private bool _isExecutionInProgress;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.
    /// </summary>
    /// <param name="executeAsyncAction">The asynchronous action to execute when the command is invoked.</param>
    public AsyncRelayCommand(Func<object?, Task> executeAsyncAction)
    {
        _executeAsyncAction = executeAsyncAction;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return !_isExecutionInProgress;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        _ = ExecuteAsync(parameter);
    }

    /// <summary>
    ///     Executes the asynchronous action, disabling the command for the duration to prevent concurrent invocations.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private async Task ExecuteAsync(object? parameter)
    {
        _isExecutionInProgress = true;
        RaiseCanExecuteChanged();

        try
        {
            await _executeAsyncAction(parameter);
        }
        finally
        {
            _isExecutionInProgress = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    ///     Raises the <see cref="CanExecuteChanged" /> event so the UI re-evaluates command availability.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
