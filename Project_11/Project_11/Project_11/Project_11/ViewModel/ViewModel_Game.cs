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

        public BoardViewModel GameBoard { get; set; } = new();

        public ICommand SendMessageCommand { get; set; } // 메시지 전송용 커맨드
        public ICommand CreateRoomCommand { get; } // 게임방 생성창 띄우는 커맨드
        public ICommand CreateGameRoomCommand { get; } // 게임방 생성, 창으로 이동하는 커맨드
        public ICommand LogOutCommand { get; } // 로그아웃
        public ICommand QuitCommand { get; } // 게임방에서 나가기 버튼

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
        public ObservableCollection<string> GameChat { get; set; } = new();
        public ObservableCollection<Status> Status { get; set; } = new();
        public ObservableCollection<OnlineUser> OnlineUsers { get; set; } = new();
        public ObservableCollection<RoomInfo> RoomList { get; set; } = new();
        public ObservableCollection<Status> OpponentStatus { get; set; } = new();
        public ObservableCollection<CellViewModel> Board { get; set; }

        public List<Status> Players { get; set; } = new();
        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

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

        private RoomInfo _selectedRoom;
        public RoomInfo SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                _selectedRoom = value;
                OnPropertyChanged(nameof(SelectedRoom));
            }
        }

        public string CurrentRoomID { get; set; }
        public int Rating { get; set; } // 방장 레이팅

        public ViewModel_Game(Account account)
        {
            Game_Account = account;
            SendMessageCommand = new RelayCommand(SendMessage, CanSendMessage);
            CreateRoomCommand = new RelayCommand(CreateRoom);
            CreateGameRoomCommand = new RelayCommand(EnterGame);
            LogOutCommand = new RelayCommand(LogOut);
            QuitCommand = new RelayCommand(QuitGameRoom);

            Board = new ObservableCollection<CellViewModel>();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Board.Add(new CellViewModel(row, col, OnCellClicked));
                }
            }
            InitializeBoard();
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
                        case "Chat": // 로비 채팅
                            Chat(json);
                            break;
                        case "Status":
                            HandleStatus(json);
                            break;
                        case "InfoList": // 유저 로그인, 로비 도달할 때
                            HandleInfoList(json);
                            break;
                        case "RoomList": // 새로운 방이 만들어질 때
                            UpdateRoomList(json);
                            break;
                        case "GameStart":
                            HandleGame(json);
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
                    Type = "Login",
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

        private void InitializeBoard()
        {
            Board = new ObservableCollection<CellViewModel>();
            
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var cell = new CellViewModel(row, col, OnCellClicked);
                    Board.Add(cell);
                }
            }

            
        }

        private void SetStone(int row, int col, string color)
        {
            var cell = Board.FirstOrDefault(c => c.Row == row && c.Column == col);
            if (cell != null)
            {
                cell.Stone = color;
            }
        }

        private void OnCellClicked(CellViewModel cell)
        {
            Console.WriteLine($"Clicked: ({cell.Row}, {cell.Column})");
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
        public void HandleStatus(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);
            Rating = data.Rating;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Status.Clear();
                Status.Add(new Status
                {
                    Name = data.Name,
                    TotalMatch = data.TotalMatch,
                    Win = data.Win,
                    Lose = data.Lose,
                    WinRate = data.TotalMatch > 0 ? (double)data.Win / data.TotalMatch * 100 : 0,
                    Rating = data.Rating
                });
            });
        }
        // 유저 전적 정보
        public void HandleInfoList(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnlineUsers.Clear();
                foreach (var user in data.Users)
                {
                    OnlineUsers.Add(new OnlineUser
                    {
                        Name = user.Name,
                        Rating = user.Rating,
                        IsPlaying = user.IsPlaying
                    });
                }

                RoomList.Clear();
                foreach (var room in data.Rooms)
                {
                    RoomList.Add(new RoomInfo
                    {
                        Title = room.Title,
                        RatingLimit = room.RatingLimit,
                        Host = room.Host,
                        CreatedTime = room.CreatedTime
                    });
                }
            });
        }
        public void HandleGame(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);
            Status opponent;

            // 상대방 구별
            if (Game_Account.Name == data.Players[0].Name)
            {
                opponent = data.Players[1];
            }
            else
            {
                opponent = data.Players[0];
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                GameChat.Add($"게임이 시작되었습니다!!");
                GameChat.Add($"[{data.Players[0].Name} ({data.Players[0].Rating})] VS [{data.Players[1].Name} ({data.Players[1].Rating})]");

                OpponentStatus.Clear();
                OpponentStatus.Add(new Status
                {
                    Name = opponent.Name,
                    TotalMatch = opponent.TotalMatch,
                    Win = opponent.Win,
                    Lose = opponent.Lose,
                    WinRate = opponent.TotalMatch > 0 ? (double)opponent.Win / opponent.TotalMatch * 100 : 0,
                    Rating = opponent.Rating
                });

                IsPlaying = data.Players[0].IsPlaying;
            });
        }
        public void UpdateRoomList(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            if (data.Host == Game_Account.Name) // 여기가 방장이면
            {
                CurrentRoomID = data.RoomID;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                RoomList.Clear();

                foreach (var room in data.Rooms)
                {
                    RoomList.Add(new RoomInfo
                    {
                        RoomID = room.RoomID,
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
                _createRoomWindow = new CreateRoom(this);
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

                if (rating > Rating)
                {
                    ShowMessage($"입력한 레이팅 제한이 본인의 레이팅보다 높습니다. (내 레이팅: {Rating})");
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
                var gameRoom = new GameRoom(this);
                gameRoom.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is CreateRoom || window is Game)
                    {
                        if (window != gameRoom)
                            window.Close();
                    }
                }
            });
        }

        public void JoinRoom(Data room)
        {
            if (room == null)
                return;

            if (!int.TryParse(room.RatingLimit, out int limit) || room.Rating >= limit)
            {
                var joinData = new Data
                {
                    Type = "JoinRoom",
                    RoomID = room.RoomID,
                    Title = room.Title,
                    ID = Game_Account.ID,
                    Name = Game_Account.Name
                };

                string json = JsonConvert.SerializeObject(joinData);
                byte[] data = Encoding.UTF8.GetBytes(json);
                _stream.Write(data, 0, data.Length);

                CurrentRoomID = room.RoomID;
                OpenGameRoom();
            }
            else
            {
                ShowMessage("레이팅이 부족해서 입장할 수 없습니다!");
            }
        }

        private void QuitGameRoom()
        {
            if (_client != null && _client.Connected)
            {
                var leaveData = new Data
                {
                    Type = "LeaveRoom",
                    RoomID = CurrentRoomID,
                    ID = Game_Account.ID,
                    Name = Game_Account.Name
                };

                string json = JsonConvert.SerializeObject(leaveData);
                byte[] data = Encoding.UTF8.GetBytes(json);
                _stream.Write(data, 0, data.Length);
            }

            CurrentRoomID = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var GameLobby = new Game(this);
                GameLobby.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameRoom)
                    {
                        window.Close();
                        break;
                    }
                }
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
                    ShowMessage($"안녕히 가십시오 {Game_Account.Name} 님!");
                    var loginWindow = new Login();
                    loginWindow.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Game)
                        {
                            window.Close();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"로그아웃 실패: {ex.Message}");
            }
        }
    }
}
