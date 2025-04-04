using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11_Server.Model
{
    public class Status
    {
        public string ID { get; set; } // 외래키
        public string Name { get; set; } // 닉네임
        public int Match { get; set; } // 총 매치 진행 수
        public int Win { get; set; } // 승리 횟수
        public int Lose { get; set; } // 패배 횟수
        public int Rating { get; set; } // 점수
    }
}
