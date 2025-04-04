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
using Newtonsoft.Json;
using System.Windows.Input;

namespace Project_11.ViewModel
{
    public class ViewModel_Game : INotifyPropertyChanged
    {
        private string address = "127.0.0.1";
        private int port_game = 0002;

        public ICommand ConnectCommand { get; set; }
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

        public async void OnLoaded()
        {
            await ConnectToGameServer();
        }

        private async Task ConnectToGameServer()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(address, port_game);

                    using (NetworkStream stream = client.GetStream())
                    {
                        string json = JsonConvert.SerializeObject(Game_Account);
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(data, 0, data.Length);

                        // 서버 응답 기다리는 부분도 가능하면 처리
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        // 예: Console.WriteLine($"게임 서버 응답: {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"게임 서버 연결 실패: {ex.Message}");
            }
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
