using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_11_Server.Model;
using Project_11_Server.View;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;

namespace Project_11_Server.Controller
{
    public class Game_Server
    {
        private TcpListener _listener;
        private List<ClientHandler> _clients = new();
    }

    public class ClientHandler
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Game_Server _server;
        private byte[] _buffer = new byte[2048];

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

                    
                }
            }
        }
    }
}
