using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Project_11.ViewModel.Commands;
using System.Windows;
using Project_11.Model;
using System.ComponentModel;

namespace Project_11.ViewModel
{
    public class ViewModel_Game : INotifyPropertyChanged
    {
        private string address = "127.0.0.1";
        private int port_game = 0002;

        public Command_Account newCommand { get; set; }
        private Account _Game_Account;
        public Account Game_Account
        {
            get { return _Game_Account; }
            set
            {
                _Game_Account = value;
                OnPropertyChanged(nameof(Game_Account));
            }
        }

        public ViewModel_Game(Account account)
        {
            Game_Account = account;
        }

        private async Task ConnectToGameServer()
        {

        }

        public void CurrentUser(object obj)
        {
            
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
