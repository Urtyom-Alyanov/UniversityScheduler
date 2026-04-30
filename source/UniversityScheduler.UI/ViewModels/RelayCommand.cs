using System.Windows.Input;

namespace UniversityScheduler.UI.ViewModels;

public class RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) : ICommand {
  public event EventHandler? CanExecuteChanged {
    add => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
  }

  public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
  public void Execute(object? parameter) => execute(parameter);
}

public class RelayCommand<T>(Action<T?> execute, Predicate<T?>? canExecute = null) : ICommand {
  public event EventHandler? CanExecuteChanged {
    add => CommandManager.RequerySuggested += value;
    remove => CommandManager.RequerySuggested -= value;
  }

  public bool CanExecute(object? parameter) => canExecute?.Invoke((T?)parameter) ?? true;
  public void Execute(object? parameter) => execute((T?)parameter);
}
