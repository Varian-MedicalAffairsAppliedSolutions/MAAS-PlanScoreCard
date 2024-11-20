using System;
using System.Windows.Input;

namespace ScorecardVisualizer.Services.Commands
{
    internal abstract class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged(object parameter)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
