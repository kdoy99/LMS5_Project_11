using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Project_11.View;

namespace Project_11.ViewModel.Commands
{
    public class Command_Account : ICommand
    {
        // 서버 연결하기 위해 매개변수 필요없는 Func<Task> 사용
        private readonly Func<Task> _execute;

        public Command_Account(Func<Task> ViewModel_Method)
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
            await _execute.Invoke();
        }
    }
}
