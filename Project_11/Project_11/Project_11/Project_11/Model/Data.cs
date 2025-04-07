using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11.Model
{
    public class Data
    {
        public string Type { get; set; }
        public object Body { get; set; }
        public string ID { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Name { get; set; } // 닉네임
        public int TotalMatch { get; set; } // 총 매치 진행 수
        public int Win { get; set; } // 승리 횟수
        public int Lose { get; set; } // 패배 횟수
        public int Rating { get; set; } // 점수
    }
}
