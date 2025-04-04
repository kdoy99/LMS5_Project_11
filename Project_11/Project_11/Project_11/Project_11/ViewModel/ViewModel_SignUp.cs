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
        private int port = 0001;
        public Command_SignUp Command_SignUp { get; set; }
        private Account _acccount;

        // 프로퍼티
        public Account account
        {
            get {  return _acccount; }
            set
            {
                _acccount = value;
                OnPropertyChanged(nameof(account));
            }
        }

        public ViewModel_SignUp()
        {
            account = new Account();
            // 서버 연결 커맨드
            Command_SignUp = new Command_SignUp(async () => await ConnectToServer());
        }
        public void DisplayMessage_New(string message)
        {
            MessageBox.Show(message);
        }
        public string SerializeAccount() // 클래스 넘기기 위한 직렬화
        {
            return JsonSerializer.Serialize(account);
        }
        public async Task ConnectToServer() // 서버 연결
        {
            try
            {
                string json = SerializeAccount();

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(address, port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(data, 0, data.Length);

                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 서버 응답 기다림
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        
                        DisplayMessage_New(receivedMessage);
                        if (receivedMessage.StartsWith("계정")) // 회원가입 완료
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (Window window in Application.Current.Windows)
                                {
                                    if (window is SignUp)
                                    {
                                        window.Close();
                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage_New($"서버 연결 중 오류 발생! : {ex.Message}");
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
