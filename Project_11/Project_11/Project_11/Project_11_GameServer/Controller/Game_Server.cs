using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_11_GameServer.Model;
using Project_11_GameServer.View;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using System.Diagnostics.Eventing.Reader;

namespace Project_11_GameServer.Controller
{
    public class Game_Server
    {
        private TcpListener _listener;
        private List<ClientHandler> _clients = new();
        private int port = 0002;

        private Log log = new Log();

        // 온라인 유저 리스트
        public List<OnlineUser> OnlineUsers()
        {
            return _clients
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .Select(c => new OnlineUser
                {
                    Name = c.Name,
                    Rating = c.Rating
                }).ToList();
        }
        // 현재 존재하는 게임방 리스트
        private List<Data> _rooms = new();

        public void AddRoom(Data roomData)
        {
            _rooms.Add(roomData);
        }
        public void RemoveRoom(string roomID)
        {
            _rooms.RemoveAll(r => r.RoomID == roomID);
            RoomMembers.Remove(roomID);
        }

        public List<Data> GetRoomList()
        {
            return _rooms;
        }

        // 게임방 별 유저 추격용 딕셔너리
        public Dictionary<string, List<ClientHandler>> RoomMembers = new();

        public async Task StartServer()
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            log.DisplayLog("게임 서버 시작");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                var handler = new ClientHandler(client, this);
                _clients.Add(handler);
                _ = handler.ProcessAsync();
            }
        }

        public void Broadcast(string json)
        {
            foreach (var client in _clients)
            {
                client.Send(json);
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            _clients.Remove(client);
        }
    }

    // 의존성 분리
    public interface INetwork
    {
        void Send(string json);
    }

    public class ClientHandler : INetwork
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Game_Server _server;
        private byte[] _buffer = new byte[2048];

        private Log log = new Log();

        public string Name { get; set; } // 유저 리스트 용, 닉네임
        public int Rating { get; set; } // 유저 리스트 용, 점수

        public ClientHandler(TcpClient client, Game_Server server)
        {
            _client = client;
            _server = server;
            _stream = _client.GetStream();
        }

        public async Task ProcessAsync()
        {
            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string json = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                    log.DisplayLog(json);

                    await HandleMessage(json);
                }
            }
            catch (Exception ex)
            {
                log.DisplayLog($"클라이언트 접속 오류 : {ex.Message}");
            }
            finally
            {
                _stream.Dispose();
                _client.Dispose();
                _server.RemoveClient(this);
                // 종료시에도 유저 목록 전송
                string userJson = InfoList();
                _server.Broadcast(userJson);
            }
        }

        private async Task HandleMessage(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            switch(data.Type)
            {
                case "InfoList": // 유저별 전적, 유저 리스트, 게임방 리스트 전송
                    SendInfoList(data.ID);
                    break;
                case "Chat": // 채팅 전송
                    _server.Broadcast(json);
                    break;
                case "CreateRoom":
                    HandleCreateRoom(data);
                    break;
                case "JoinRoom":
                    HandleJoinRoom(data);
                    break;
                case "Game":

                    break;
                default:
                    
                    break;
            }
        }

        private void SendInfoList(string id)
        {
            var status = GetUserStatus(id);
            var users = _server.OnlineUsers();
            var rooms = _server.GetRoomList();


            var response = new Data
            {
                Type = "InfoList",
                ID = status.ID,
                Name = status.Name,
                TotalMatch = status.TotalMatch,
                Win = status.Win,
                Lose = status.Lose,
                Rating = status.Rating,
                Users = users,
                Rooms = rooms
            };

            string json = JsonConvert.SerializeObject(response);
            _server.Broadcast(json);
        }

        private Status GetUserStatus(string id)
        {
            Status userStatus = null;

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;Database=project_11;Uid=root;Pwd=1234;"))
            {
                connection.Open();
                // 유저 정보 테이블 생성

                string statusQuery = "SELECT ID, Name, TotalMatch, Win, Lose, Rating FROM status WHERE ID = @ID";

                using (MySqlCommand cmd = new MySqlCommand(statusQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userStatus = new Status
                            {
                                ID = reader.GetString("ID"),
                                Name = reader.GetString("Name"),
                                TotalMatch = reader.GetInt32("TotalMatch"),
                                Win = reader.GetInt32("Win"),
                                Lose = reader.GetInt32("Lose"),
                                Rating = reader.GetInt32("Rating")
                            };
                        }
                    }
                }
            }

            Name = userStatus.Name;
            Rating = userStatus.Rating;

            return userStatus;
        }

        private void HandleCreateRoom(Data data)
        {
            string title = data.Title;

            var roomData = new Data
            {
                Type = "RoomList",
                RoomID = Guid.NewGuid().ToString(), // Guid.NewGuid() => 고유 ID 생성 기능
                Title = data.Title,
                RatingLimit = data.RatingLimit,
                Host = data.Host,
                CreatedTime = DateTime.Now.ToString("tt hh:mm")
            };

            _server.AddRoom(roomData);
            log.DisplayLog($"게임방 생성 완료! {roomData.RoomID}, {roomData.Title}");

            var fullRoomData = new Data
            {
                Type = "RoomList",
                Rooms = _server.GetRoomList()
            };
            
            string broadcastJson = JsonConvert.SerializeObject(fullRoomData);
            _server.Broadcast(broadcastJson);
        }

        private void HandleJoinRoom(Data data)
        {
            var room = _server.GetRoomList().FirstOrDefault(r => r.RoomID == data.RoomID);
            if (room == null)
                return;

            if (!_server.RoomMembers.ContainsKey(room.RoomID))
                _server.RoomMembers[room.RoomID] = new List<ClientHandler>();

            _server.RoomMembers[room.RoomID].Add(this);
            log.DisplayLog($"[{room.RoomID}] [{room.Title}]에 [{room.ID}] {room.Name}님 입장!");
            
            if (_server.RoomMembers[room.RoomID].Count >= 2 || 
                _server.RoomMembers[room.RoomID].Count == 0)
            {
                _server.RemoveRoom(room.RoomID);
            }

            string json = InfoList();
            _server.Broadcast(json);

        }
        private string InfoList()
        {
            var users = _server.OnlineUsers();
            var rooms = _server.GetRoomList();

            var response = new Data
            {
                Type = "InfoList",
                Users = users,
                Rooms = rooms
            };

            return JsonConvert.SerializeObject(response);
        }

        public async void Send(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                log.DisplayLog($"메시지 전송 실패: {ex.Message}");
            }
        }
    }
}
