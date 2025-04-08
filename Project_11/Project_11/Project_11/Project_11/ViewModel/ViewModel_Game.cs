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
using System.Collections.ObjectModel;

namespace Project_11.ViewModel
{
    public class ViewModel_Game : INotifyPropertyChanged
    {
        private string address = "127.0.0.1";
        private int port = 0002;
        private TcpClient _client;
        private NetworkStream _stream;

        public ICommand ConnectCommand { get; set; }
        public ICommand SendMessageCommand { get; set; } // 메시지 전송용 커맨드

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
        public ObservableCollection<string> ChatMessages { get; set; } = new();
        public ObservableCollection<Status> UserStatusModel { get; set; } = new();
        public ObservableCollection<OnlineUser> OnlineUsers { get; set; } = new();

        private string _chatMessage;
        public string ChatMessage
        {
            get => _chatMessage;
            set
            {
                _chatMessage = value;
                OnPropertyChanged(nameof(ChatMessage));
            }
        }

        public ViewModel_Game(Account account)
        {
            Game_Account = account;
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatMessage))
            {
                SendToServer(ChatMessage);
                ChatMessage = string.Empty; // 입력창 초기화
            }
        }

        private bool CanSendMessage()
        {
            return !string.IsNullOrWhiteSpace(ChatMessage);
        }

        private async Task ListenAsync()
        {
            byte[] buffer = new byte[2048];

            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var data = JsonConvert.DeserializeObject<Data>(json);

                    switch (data.Type)
                    {
                        case "Chat":
                            Chat(json);
                            break;
                        case "UserInfo":
                            UserStatus(json);
                            break;
                        case "OnlineUser":
                            CurrentUser(json);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 수신 실패: {ex.Message}");
            }
        }

        public async void OnLoaded()
        {
            await ConnectToGameServer();
        }

        private async Task ConnectToGameServer()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(address, port);
                _stream = _client.GetStream();

                var loginData = new Data
                {
                    Type = "UserInfo",
                    ID = Game_Account.ID
                };
                
                string json = JsonConvert.SerializeObject(loginData);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(data, 0, data.Length);

                // 서버 응답 기다리는 부분
                await Task.Run(ListenAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"게임 서버 연결 실패: {ex.Message}");
            }
        }

        private void SendToServer(string message)
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        Type = "Chat",
                        Content = message,
                        Sender = Game_Account.Name
                    });

                    byte[] data = Encoding.UTF8.GetBytes(json);
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"메시지 전송 실패: {ex.Message}");
                }
            }
        }

        public void Chat(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add($"[{data.Sender}] {data.Content}");
            });
        }

        public void UserStatus(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Application.Current.Dispatcher.Invoke(() =>
            {
                UserStatusModel.Clear();
                UserStatusModel.Add(new Status
                {
                    Name = data.Name,
                    TotalMatch = data.TotalMatch,
                    Win = data.Win,
                    Lose = data.Lose,
                    Rate = data.TotalMatch > 0 ? data.TotalMatch / data.Win * 100 : 0,
                    Rating = data.Rating,
                });
            });
        }

        public void CurrentUser(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnlineUsers.Clear();
                foreach (var user in data.Users)
                {
                    OnlineUsers.Add(new OnlineUser
                    {
                        UserName = user.UserName,
                        Rating = user.Rating
                    });
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
