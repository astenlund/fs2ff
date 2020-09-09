// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System;
using System.Windows.Input;

namespace fs2ff
{
    public class ActionCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _predicate;

        public ActionCommand()
        {
            _action = () => { };
            _predicate = () => true;
        }

        public ActionCommand(Action action)
        {
            _action = action;
            _predicate = () => true;
        }

        public ActionCommand(Action action, Func<bool> predicate)
        {
            _action = action;
            _predicate = predicate;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? _) => _predicate();

        public void Execute(object? parameter) => _action();

        public void TriggerCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }

    public class ActionCommand<T> : ICommand where T : struct
    {
        private readonly Action<T?> _action;
        private readonly Func<T?, bool> _predicate;

        public ActionCommand()
        {
            _action = _ => { };
            _predicate = _ => true;
        }

        public ActionCommand(Action<T?> action)
        {
            _action = action;
            _predicate = _ => true;
        }

        public ActionCommand(Action<T?> action, Func<T?, bool> predicate)
        {
            _action = action;
            _predicate = predicate;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => parameter is T param ? _predicate(param) : _predicate(null);

        public void Execute(object? parameter)
        {
            if (parameter is T param) _action(param);
            else                      _action(null);
        }

        public void TriggerCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
