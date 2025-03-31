using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Project_11.ViewModel.Commands;
using Project_11.Model;

namespace Project_11.ViewModel
{
    public class ViewModel_Base
    {
        public Command LoginCommand { get; set; }
        public Account UserAccount { get; set; }

        public ViewModel_Base()
        {
            LoginCommand = new Command(DisplayMessage);
            UserAccount = new Account();
        }

        public void DisplayMessage(object obj)
        {
            string text = $"ID: {UserAccount.ID}\nPW: {UserAccount.Password}";
            MessageBox.Show(text);
        }
    }
}
