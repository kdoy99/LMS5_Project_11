using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11_GameServer.Model
{
    public class OnlineUser
    {
        public string Name { get; set; }
        public int Rating { get; set; }
        public bool IsPlaying { get; set; }
    }
}
