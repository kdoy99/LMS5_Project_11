using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Project_11.View;

namespace Project_11.ViewModel.Commands
{
    public class Command_SignUp : ICommand
    {
        private readonly Func<string, Task> _execute;

        public Command_SignUp(Func<string, Task> ViewModel_Method)
        {
            _execute = ViewModel_Method;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            if (parameter is string message)
            {
                _execute.Invoke(message);
            }
        }
    }
}
