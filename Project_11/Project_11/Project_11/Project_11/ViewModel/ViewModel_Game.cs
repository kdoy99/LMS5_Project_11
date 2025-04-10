﻿using System;
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
using Project_11.View;
using System.Printing;

namespace Project_11.ViewModel
{
    public class ViewModel_Game : BaseViewModel
    {
        private string address = "127.0.0.1";
        private int port = 0002;
        private TcpClient _client;
        private NetworkStream _stream;

        public ICommand SendMessageCommand { get; set; } // 메시지 전송용 커맨드
        public ICommand CreateRoomCommand { get; } // 게임방 생성창 띄우는 커맨드
        public ICommand CreateGameRoomCommand { get; } // 게임방 생성, 창으로 이동하는 커맨드
        public ICommand LogOutCommand { get; } // 로그아웃

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
        public ObservableCollection<Data> RoomList { get; set; } = new();

        private string _roomTitle;
        public string RoomTitle
        {
            get => _roomTitle;
            set
            {
                _roomTitle = value;
                OnPropertyChanged(nameof(RoomTitle));
            }
        }

        private string _ratingLimit;
        public string RatingLimit
        {
            get => _ratingLimit;
            set
            {
                _ratingLimit = value;
                OnPropertyChanged(nameof(RatingLimit));
            }
        }

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
            CreateRoomCommand = new RelayCommand(CreateRoom);
            CreateGameRoomCommand = new RelayCommand(EnterGame);
            LogOutCommand = new RelayCommand(LogOut);
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
                        case "InfoList":
                            HandleInfoList(json);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_client == null || _client?.Connected == false || ex is ObjectDisposedException)
                    return;

                ShowErrorMessage($"데이터 수신 실패: {ex.Message}");
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
                ShowErrorMessage($"게임 서버 연결 실패: {ex.Message}");
            }
        }
        // 채팅
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
        private void SendToServer(string message)
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(new Data
                    {
                        Type = "Chat",
                        Content = message,
                        Name = Game_Account.Name
                    });

                    byte[] data = Encoding.UTF8.GetBytes(json);
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"메시지 전송 실패: {ex.Message}");
                }
            }
        }
        
        public void Chat(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add($"[{data.Name}] {data.Content}");
            });
        }
        // 유저 전적 정보
        public void HandleInfoList(string json)
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

                OnlineUsers.Clear();
                foreach (var user in data.Users)
                {
                    OnlineUsers.Add(new OnlineUser
                    {
                        Name = user.Name,
                        Rating = user.Rating
                    });
                }

                RoomList.Clear();
                foreach (var room in data.Rooms)
                {
                    RoomList.Add(new Data
                    {
                        Title = room.Title,
                        RatingLimit = room.RatingLimit,
                        Host = room.Host,
                        CreatedTime = room.CreatedTime
                    });
                }
            });
        }

        // 중복 윈도우 방지용 필드
        private Window? _createRoomWindow;

        // 방 생성 메소드
        private void CreateRoom()
        {
            if (_createRoomWindow == null || !_createRoomWindow.IsVisible)
            {
                _createRoomWindow = new CreateRoom();
                _createRoomWindow.Show();
            }
            else
            {
                _createRoomWindow.Activate(); // 이미 열려 있는 창을 앞으로
            }
            
        }

        private void EnterGame()
        {
            if (string.IsNullOrWhiteSpace(RoomTitle))
            {
                ShowMessage("방 제목을 입력해주세요.");
                return;
            }

            int rating = 0;
            if (!string.IsNullOrWhiteSpace(RatingLimit))
            {
                if (!int.TryParse(RatingLimit, out rating))
                {
                    ShowMessage("레이팅 제한은 숫자로 입력해주세요.");
                    return;
                }
            }

            SendRoomDataToServer(RoomTitle, rating.ToString());
            OpenGameRoom();
        }

        private void SendRoomDataToServer(string title, string rating)
        {
            var roomData = new Data
            {
                Type = "CreateRoom",
                Title = title,
                RatingLimit = rating,
                Host = Game_Account.Name
            };

            string json = JsonConvert.SerializeObject(roomData);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            _stream.Write(buffer, 0, buffer.Length);
        }

        private void OpenGameRoom()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is CreateRoom || window is Game)
                    {
                        window.Close();
                    }
                }

                var gameRoom = new GameRoom();
                gameRoom.DataContext = new ViewModel_Game(Game_Account);
                gameRoom.Show();
            });
        }

        // 로그아웃
        private void LogOut()
        {
            try
            {
                _stream?.Close();
                _client?.Close();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Game)
                        {
                            ShowMessage($"안녕히 가십시오 {Game_Account.Name} 님!");
                            window.Close();
                            break;
                        }
                    }

                    var loginWindow = new Login();
                    loginWindow.Show();
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"로그아웃 실패: {ex.Message}");
            }
        }
    }
}
