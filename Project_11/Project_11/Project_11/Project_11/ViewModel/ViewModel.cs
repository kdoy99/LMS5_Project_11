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
    class ViewModel
    {
        public Command cmd { get; set; }
        public Account account { get; set; }

        public ViewModel()
        {
            cmd = new Command(DisplayMessage);
            account = new Account();
            this.DataContext = account;
        }

        public void DisplayMessage(string textbox_input)
        {
            string text = $"ID: {textbox_input}\nPW: {textbox_input}";
            MessageBox.Show(textbox_input);
        }

        public void DisplayAccount(Account account)
        {
            string text = $"ID: {account.id}\nPW: {account.password}";
            MessageBox.Show(text);
        }
    }
}
