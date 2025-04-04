using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Project_11.View;

namespace Project_11.ViewModel.Commands
{
    // string문 받아서 처리하는 Command
    public class Command_Window : ICommand
    {
        private Action<string> _execute;

        public Command_Window(Action<string> ViewModel_Method)
        {
            _execute = ViewModel_Method;
        }
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute.Invoke(parameter as string);
        }
    }
}
