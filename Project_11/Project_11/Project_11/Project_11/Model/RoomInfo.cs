using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11.Model
{
    public class RoomInfo
    {
        public string RoomID { get; set; } // 방 ID
        public string Title { get; set; } // 방 제목
        public DateTime CreatedTime { get; set; } // 만들어진 시간
        public string Host { get; set; } // 방장 닉네임
        public int HostRating { get; set; } // 방장 레이팅
        public string RatingLimit { get; set; } // 레이팅 제한
        public string DisplayTime => CreatedTime.ToString("tt h:mm");
        public string HostInfo => $"{Host} [{HostRating}]";
    }
}
