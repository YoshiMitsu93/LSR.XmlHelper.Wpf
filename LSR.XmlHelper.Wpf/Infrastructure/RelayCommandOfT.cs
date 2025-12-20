using System;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public sealed class RelayCommandOfT<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommandOfT(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute is null)
                return true;

            if (parameter is null)
                return _canExecute(default);

            if (parameter is T typed)
                return _canExecute(typed);

            return _canExecute((T?)Convert.ChangeType(parameter, typeof(T)));
        }

        public void Execute(object? parameter)
        {
            if (parameter is null)
            {
                _execute(default);
                return;
            }

            if (parameter is T typed)
            {
                _execute(typed);
                return;
            }

            _execute((T?)Convert.ChangeType(parameter, typeof(T)));
        }
    }
}
