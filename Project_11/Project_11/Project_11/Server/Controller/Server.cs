using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Project_11_Server.Model;
using Project_11_Server.View;

namespace Project_11_Server.Controller
{
    public class Server
    {
        private TcpListener _listener;
        private int port = 5457;

        Log log = new Log();

        public Server()
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartServer()
        {
            _listener.Start();
            log.DisplayLog("서버 시작");

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                log.DisplayLog($"클라이언트 연결됨!\n" +
                    $"연결된 클라이언트: {client.Client.RemoteEndPoint}");

                await HandleClientAsync(client);
            }
            
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    log.DisplayLog($"받은 메시지: {receivedMessage}");

                    string response = "서버 응답: " + receivedMessage;
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    log.DisplayLog($"클라이언트에 메시지 전송: {response}");
                }
            }
            catch (Exception ex)
            {
                log.DisplayLog($"클라이언트와 통신 중 오류 발생: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }
}
