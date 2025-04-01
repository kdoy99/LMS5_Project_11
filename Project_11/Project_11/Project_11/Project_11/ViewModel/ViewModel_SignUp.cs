using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Project_11.Model;
using Project_11.View;
using Project_11.ViewModel.Commands;
using System.Text.Json;

namespace Project_11.ViewModel
{
    // 회원가입 서버 연결 클래스
    public class ViewModel_SignUp : INotifyPropertyChanged
    {
        private string address = "127.0.0.1";
        private int port = 5457;
        public Command_SignUp Command_SignUp { get; set; }
        private Account _acccount;
        public Account account
        {
            get {  return _acccount; }
            set
            {
                _acccount = value;
                OnPropertyChanged(nameof(Account));
            }
        }

        public ViewModel_SignUp()
        {
            Command_SignUp = new Command_SignUp(_ => ConnectToServer(SerializeAccount(account)));
            account = new Account();
        }
        public void DisplayMessage_New(string message)
        {
            MessageBox.Show(message);
        }
        public string SerializeAccount(Account account)
        {
            return JsonSerializer.Serialize(account);
        }
        public async Task ConnectToServer(string json)
        {
            try
            {
                using (TcpClient client = new TcpClient(address, port))
                {
                    DisplayMessage_New($"서버에 연결됨: " +
                    $"{client.Client.RemoteEndPoint}");

                    NetworkStream stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(data, 0, data.Length);
                    DisplayMessage_New($"서버로 보낸 메시지: {json}");

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
