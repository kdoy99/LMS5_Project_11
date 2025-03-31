using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_11_Server.Controller;

namespace Project_11_Server.Controller
{
    class Execute
    {
        static async Task Main(string[] args)
        {
            Server server = new Server();
            await server.StartServer();
        }
    }
}
