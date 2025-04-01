using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Project_11_Server.Model;
using Project_11_Server.View;
using System.Text.Json;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1;

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
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    log.DisplayLog($"받은 메시지: {json}");

                    Account account = JsonSerializer.Deserialize<Account>(json);
                    string isSuccess = ConnectDB(account);

                    string response;
                    if (isSuccess == "성공")
                    {
                        response = $"계정 생성 완료!\n" +
                            $"아이디: {account.ID}\n" +
                            $"닉네임: {account.Name}";
                    }
                    else if (isSuccess == "중복")
                    {
                        response = $"중복된 ID, 닉네임 또는 연락처가 존재합니다.";
                    }
                    else
                    {
                        response = $"알 수 없는 오류로 계정 생성 실패..";
                    }

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

        private string ConnectDB(Account account)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection("Server = localhost;Database=project_11;Uid=root;Pwd=1234;"))
                {
                    connection.Open();
                    log.DisplayLog("DB 연결 성공");

                    // 중복 검사
                    string selectQuery = "SELECT COUNT(*) FROM users WHERE ID = @ID OR Name = @Name OR Contact =@Contact";
                    using (MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection))
                    {
                        selectCmd.Parameters.AddWithValue("@ID", account.ID);
                        selectCmd.Parameters.AddWithValue("@Name", account.Name);
                        selectCmd.Parameters.AddWithValue("@Contact", account.Contact);

                        int count = Convert.ToInt32(selectCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            log.DisplayLog("중복된 ID, 닉네임 또는 연락처가 존재합니다.");
                            return "중복";
                        }
                    }

                    string insertQuery = "INSERT INTO users (ID, Password, Name, Contact) VALUES (@ID, @Password, @Name, @Contact)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@ID", account.ID);
                        insertCmd.Parameters.AddWithValue("@Password", account.Password);
                        insertCmd.Parameters.AddWithValue("@Name", account.Name);
                        insertCmd.Parameters.AddWithValue("@Contact", account.Contact);

                        int rowsAffected = insertCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            log.DisplayLog("회원가입 성공!");
                            return "성공";
                        }
                        else
                        {
                            log.DisplayLog("회원가입 실패..");
                            return "실패";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.DisplayLog($"DB 연결 중 오류 발생: {ex.Message}");
                return "실패";
            }
        }
    }
}
