namespace BankingApp.Desktop.Shared.Commands;

using System;
using System.Windows.Input;

/// <summary>
///     A basic command that delegates execution to a provided action.
/// </summary>
public partial class RelayCommand : ICommand
{
    private readonly Action<object?> _executeAction;
    private readonly Func<object?, bool>? _canExecutePredicate;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
    /// </summary>
    /// <param name="executeAction">The action to execute when the command is invoked.</param>
    /// <param name="canExecutePredicate">An optional predicate that determines whether the command can execute.</param>
    public RelayCommand(Action<object?> executeAction, Func<object?, bool>? canExecutePredicate = null)
    {
        _executeAction = executeAction;
        _canExecutePredicate = canExecutePredicate;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return _canExecutePredicate == null || _canExecutePredicate(parameter);
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        _executeAction(parameter);
    }

    /// <summary>
    ///     Raises the <see cref="CanExecuteChanged" /> event so the UI re-evaluates command availability.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
