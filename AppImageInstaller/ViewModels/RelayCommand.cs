using System.Windows.Input;

namespace AppImageInstaller.ViewModels;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action execute = execute;
    private readonly Action<object?>? executeWithParameter;
    private readonly Func<bool>? canExecute = canExecute;

    public RelayCommand(Action<object?> executeWithParameter, Func<bool>? canExecute = null)
        : this(() => { }, canExecute)
    {
        this.executeWithParameter = executeWithParameter;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

    public void Execute(object? parameter)
    {
        if (executeWithParameter is not null)
        {
            executeWithParameter(parameter);
            return;
        }

        execute();
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
