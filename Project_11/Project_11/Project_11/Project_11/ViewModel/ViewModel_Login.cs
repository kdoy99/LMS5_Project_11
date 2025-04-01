using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Project_11.ViewModel.Commands;
using Project_11.Model;
using Project_11.View;
using System.Windows.Input;

namespace Project_11.ViewModel
{
    public class ViewModel_Login
    {
        public Account UserAccount { get; set; }
        public ICommand OpenLoginCommand { get; set; }
        public ICommand OpenAccessCommand { get; set; }

        public ViewModel_Login()
        {
            UserAccount = new Account();

            OpenLoginCommand = new Command_Login(Login);
            OpenAccessCommand = new Command_Login(Access);
        }
        private void Login(object obj)
        {
            string text = $"로그인 성공!\n" +
                $"ID: {UserAccount.ID}\nPW: {UserAccount.Password}";
            MessageBox.Show(text);

            ViewModel_Game viewModel = new ViewModel_Game(UserAccount);
            Game client = new Game(viewModel);
            client.Show();
        }
        private void Access(object obj)
        {
            SignUp newAccount = new SignUp();
            newAccount.Show();
        }
    }
}
