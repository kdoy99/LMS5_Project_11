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
                .GroupBy(c => c.Name)
                .Select(c => new OnlineUser
                {
                    Name = c.First().Name,
                    Rating = c.First().Rating,
                    IsPlaying = c.First().IsPlaying
                }).ToList();
        }
        // 현재 존재하는 게임방 리스트
        private List<RoomInfo> _rooms = new();
        // 해당하는 게임방이 플레이중인지 아닌지
        public Dictionary<string, bool> RoomPlayingStatus = new();

        public void AddRoom(RoomInfo roomData)
        {
            _rooms.Add(roomData);
            RoomPlayingStatus[roomData.RoomID] = false;
        }
        public void RemoveRoom(string roomID)
        {
            _rooms.RemoveAll(r => r.RoomID == roomID);
        }

        public List<RoomInfo> GetRoomList()
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
            log.DisplayLog($"송신 : {json}");
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
        void SendToClient(ClientHandler client, Data data);
    }

    public class ClientHandler : INetwork
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Game_Server _server;
        private byte[] _buffer = new byte[2048];

        private Log log = new Log();
        private Status status = new Status();

        public string ID { get; set; } // 유저 아이디
        public string Name { get; set; } // 유저 닉네임
        public int Rating { get; set; } // 유저 점수
        public int TotalMatch { get; set; } // 총 판수
        public int Win { get; set; } // 총 승리
        public int Lose { get; set; } // 총 패배
        public bool IsPlaying { get; set; } // 플레이 유무

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
                    log.DisplayLog($"수신 : {json}");

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
                case "Login":
                    status = GetUserStatus(data.ID);
                    ResetUserStatus(status);
                    SendStatus(data.ID);
                    SendInfoList(data.ID);
                    break;
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
                case "LeaveRoom":
                    HandleLeaveRoom(data);
                    break;
                default:
                    
                    break;
            }
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
            return userStatus;
        }

        private void ResetUserStatus(Status _status)
        {
            // 안정적으로 필드에 저장
            ID = _status.ID;
            Name = _status.Name;
            Rating = _status.Rating;
            TotalMatch = _status.TotalMatch;
            Win = _status.Win;
            Lose = _status.Lose;
            IsPlaying = _status.IsPlaying;
        }

        private void SendStatus(string id)
        {
            var response = new Data
            {
                Type = "Status",
                ID = ID,
                Name = Name,
                Rating = Rating,
                TotalMatch = TotalMatch,
                Win = Win,
                Lose = Lose
            };

            string json = JsonConvert.SerializeObject(response);
            Send(json);
            log.DisplayLog($"송신 : {json}");
        }

        private void SendInfoList(string id)
        {
            var users = _server.OnlineUsers();
            var rooms = _server.GetRoomList();


            var response = new Data
            {
                Type = "InfoList",
                Users = users,
                Rooms = rooms
            };

            string json = JsonConvert.SerializeObject(response);
            _server.Broadcast(json);
            log.DisplayLog($"송신 : {json}");
        }

        private void HandleCreateRoom(Data data)
        {
            var roomData = new RoomInfo
            {
                RoomID = Guid.NewGuid().ToString(), // Guid.NewGuid() => 고유 ID 생성 기능
                Title = data.Title,
                RatingLimit = data.RatingLimit,
                Host = data.Host,
                CreatedTime = DateTime.Now
            };

            _server.AddRoom(roomData);
            _server.RoomMembers[roomData.RoomID] = new List<ClientHandler> { this };
            var Data = new Data
            {
                Type = "RoomList",
                RoomID = roomData.RoomID,
                Host = data.Host,
                Rooms = _server.GetRoomList()
            };

            string broadcastJson = JsonConvert.SerializeObject(Data);
            _server.Broadcast(broadcastJson);
            log.DisplayLog($"게임방 생성 완료! {roomData.RoomID}, {roomData.Title}");
        }

        private void HandleJoinRoom(Data data)
        {
            var room = _server.GetRoomList().FirstOrDefault(r => r.RoomID == data.RoomID); // 존재하는 방인지 검사
            if (room == null) // 방이 없으면
                return; // 돌아가기

            _server.RoomMembers[room.RoomID].Add(this); // 방에 유저 추가
            log.DisplayLog($"[{room.RoomID}] [{room.Title}]에 [{data.ID}] {data.Name}님 입장!");

            var members = _server.RoomMembers[room.RoomID];
            if (members.Count < 2) // 방이 가득차지 않았다면
                return;

            // 방에 2명이 다 들어온 뒤
            _server.RemoveRoom(room.RoomID);
            _server.RoomPlayingStatus[room.RoomID] = true;
            log.DisplayLog($"[{room.RoomID}] [{room.Title}] 방 목록에서 제거");

            members[0].IsPlaying = true;
            members[1].IsPlaying = true;
            
            string json = InfoList();
            _server.Broadcast(json);

            var player1Data = new Status
            {
                ID = members[0].ID,
                Name = members[0].Name,
                TotalMatch = members[0].TotalMatch,
                Win = members[0].Win,
                Lose = members[0].Lose,
                Rating = members[0].Rating,
                IsPlaying = members[0].IsPlaying
            };

            var player2Data = new Status
            {
                ID = members[1].ID,
                Name = members[1].Name,
                TotalMatch = members[1].TotalMatch,
                Win = members[1].Win,
                Lose = members[1].Lose,
                Rating = members[1].Rating,
                IsPlaying = members[1].IsPlaying
            };

            
            
            // 랜덤 순서
            Random rand = new Random();
            int randomColor = rand.Next(2);
            string color1;
            string color2;
            if (randomColor == 0)
            {
                color1 = "Black";
                color2 = "White";
            }
            else
            {
                color1 = "White";
                color2 = "Black";
            }

            var Data1 = new Data
            {
                Type = "GameStart",
                RoomID = room.RoomID,
                Color = color1,
                Players = new List<Status> { player1Data, player2Data }
            };

            var Data2 = new Data
            {
                Type = "GameStart",
                RoomID = room.RoomID,
                Color = color2,
                Players = new List<Status> { player2Data, player1Data }
            };

            SendToClient(members[0], Data1);
            SendToClient(members[1], Data2);
        }

        private void HandleLeaveRoom(Data data)
        {
            var members = _server.RoomMembers[data.RoomID];
            int count = members.Count;

            members.RemoveAll(m => m.Name == data.Name);

            int afterCount = members.Count;
            log.DisplayLog($"[LeaveRoom] Before: {count}, After: {afterCount}");

            if (members.Count == 0)
            {
                _server.RoomMembers.Remove(data.RoomID);
                _server.RemoveRoom(data.RoomID);
                log.DisplayLog($"[{data.RoomID}] 방장의 이탈로 방 삭제됨!");
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

        public void SendToClient(ClientHandler client, Data data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                client.Send(json);
            }
            catch (Exception ex)
            {
                log.DisplayLog($"게임 시작을 위한 데이터 전송 실패 : {ex.Message}");
            }
        }
    }
}
