﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_11.Model
{
    public class Data
    {
        public string Type { get; set; } // 데이터 종류

        public string ID { get; set; } // ID
        public string Content { get; set; } // 내용
        public string Name { get; set; } // 닉네임

        public int TotalMatch { get; set; } // 총 매치 진행 수
        public int Win { get; set; } // 승리 횟수
        public int Lose { get; set; } // 패배 횟수
        public int Rating { get; set; } // 점수

        public string RoomID { get; set; } // 고유 방 ID
        public string Title { get; set; } // 생성된 방 제목
        public string Host {  get; set; } // 방장 닉네임
        public string RatingLimit { get; set; } // 레이팅 제한
        public string CreatedTime { get; set; } // 방 생성 시간


        public string Color { get; set; } // 돌 색깔
        public List<Status> Players { get; set; } // 게임 참여 유저

        public List<OnlineUser>? Users { get; set; } // 접속 중인 유저 리스트
        public List<RoomInfo> Rooms { get; set; } // 현재 존재하는 게임방 리스트
    }
}
