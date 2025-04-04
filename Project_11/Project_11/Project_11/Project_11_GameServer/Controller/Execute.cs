namespace Project_11_GameServer.Controller
{
    class Execute
    {
        static async Task Main(string[] args)
        {
            Game_Server server = new Game_Server();
            await server.StartServer();
        }
    }
}
