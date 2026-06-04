using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Trainingsfortschritt.Web.Services { }

public class SimpleCommand : ICommand
{
    private readonly Action _execute;

    public SimpleCommand(Action execute)
        => _execute = execute;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => _execute();
}

public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;

    public AsyncCommand(Func<Task> execute)
        => _execute = execute;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => true;

    public async void Execute(object? parameter)
        => await _execute();
}
