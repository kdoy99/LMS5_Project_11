using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Project_11.ViewModel.Commands;
using System.Windows;
using Project_11.Model;

namespace Project_11.ViewModel
{
    public class ViewModel_Game
    {
        public Command_SignUp newCommand { get; set; }
        public Account Game_Account { get; set; }

        public ViewModel_Game(Account account)
        {
            Game_Account = account;
            //newCommand = new Command_SignUp(ConnectToServer);
            //account = new Account();
        }

        public void DisplayMessage_New(string message)
        {
            MessageBox.Show(message);
        }
    }
}
