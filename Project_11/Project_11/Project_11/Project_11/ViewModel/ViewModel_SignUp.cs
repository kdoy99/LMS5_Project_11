using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Project_11.Model;
using Project_11.View;
using Project_11.ViewModel.Commands;

namespace Project_11.ViewModel
{
    // 회원가입 서버 연결 클래스
    public class ViewModel_SignUp
    {
        private string address = "127.0.0.1";
        private int port = 5457;

        public Command_SignUp newCommand { get; set; }
        public Account account { get; set; }

        public ViewModel_SignUp()
        {
            newCommand = new Command_SignUp(ConnectToServer);
            account = new Account();
        }

        public async Task ConnectToServer(string message)
        {
            try
            {
                using (TcpClient client = new TcpClient(address, port))
                {
                    DisplayMessage_New($"서버에 연결됨: " +
                    $"{client.Client.RemoteEndPoint}");

                    NetworkStream stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(data, 0, data.Length);
                    DisplayMessage_New($"서버로 보낸 메시지: {message}");

                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    DisplayMessage_New($"서버로부터 받은 응답: {receivedMessage}");
                }
            }
            catch (Exception ex)
            {
                DisplayMessage_New($"서버 연결 중 오류 발생! : {ex.Message}");
            }
        }

        public void DisplayMessage_New(string message)
        {
            MessageBox.Show(message);
        }
    }
}
