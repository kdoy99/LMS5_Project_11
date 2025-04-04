using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Project_11.ViewModel.Commands;
using Project_11.Model;
using Project_11.View;
using System.Windows.Input;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace Project_11.ViewModel
{
    public class ViewModel_Login
    {
        private string address = "127.0.0.1";
        private int port_account = 0001;
        private int port_game = 0002;

        public Account UserAccount { get; set; }
        public ICommand OpenLoginCommand { get; set; } // 로그인 버튼 클릭용 커맨드
        public ICommand OpenAccessCommand { get; set; } // 회원가입 버튼 클릭용 커맨드

        public ViewModel_Login()
        {
            UserAccount = new Account();

            OpenLoginCommand = new Command_Account(async () => await ConnectToServer());
            OpenAccessCommand = new Command_Window(Access);
        }

        private void DisplayMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void Access(object obj)
        {
            SignUp signUp = new SignUp();
            signUp.Show();
        }

        private string SerializeAccount() // 클래스 넘기기 위한 직렬화
        {
            UserAccount.Type = "로그인";
            return JsonConvert.SerializeObject(UserAccount);
        }
        private async Task ConnectToServer() // 서버 연결
        {
            try
            {
                string json = SerializeAccount();

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(address, port_account);

                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(data, 0, data.Length);

                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 서버 응답 기다림
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        DisplayMessage(receivedMessage);
                        if (receivedMessage.StartsWith("로그인")) // 로그인 완료
                        {
                            ViewModel_Game viewModel = new ViewModel_Game(UserAccount);
                            Game game = new Game(viewModel);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (Window window in Application.Current.Windows)
                                {
                                    if (window is Login)
                                    {
                                        window.Close();
                                    }
                                }
                            });
                            game.Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayMessage($"서버 연결 중 오류 발생! : {ex.Message}");
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
