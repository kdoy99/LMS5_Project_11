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

        public void Broadcast(string message)
        {
            foreach (var client in _clients)
            {
                client.Send(message);
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            _clients.Remove(client);
        }
    }

    public class ClientHandler
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Game_Server _server;
        private byte[] _buffer = new byte[2048];

        private Status status;
        private Log log = new Log();

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
                _client.Close();
                _server.RemoveClient(this);
            }
        }

        private async Task HandleMessage(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            switch(data.Type)
            {
                case "UserInfo":
                    UserStatus(json);
                    break;
                case "Chat":
                    Send(json);
                    break;
                case "UserList":

                    break;
                case "RoomList":

                    break;
                case "Create_Room":

                    break;
                case "Game":

                    break;
                default:
                    
                    break;
            }
        }

        public Status UserStatus(string json)
        {
            var data = JsonConvert.DeserializeObject<Data>(json);

            Status userStatus = null;

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;Database=project_11;Uid=root;Pwd=1234;"))
            {
                connection.Open();
                // 유저 정보 테이블 생성

                string statusQuery = "SELECT ID, Name, Match, Win, Lose, Rating FROM status WHERE ID = @ID";

                using (MySqlCommand cmd = new MySqlCommand(statusQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ID", data.ID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userStatus = new Status
                            {
                                ID = reader.GetString("ID"),
                                Name = reader.GetString("Name"),
                                TotalMatch = reader.GetInt32("Match"),
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

        public void Send(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
