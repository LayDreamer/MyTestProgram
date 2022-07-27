using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Dev.Framework.MVVM
{
    public class RelayCommandBase : ICommand
    {
        Action _execute;
        Func<bool> _canExecute;
        public RelayCommandBase(Action execute) : this(execute, null)
        {
        }
        public RelayCommandBase(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}