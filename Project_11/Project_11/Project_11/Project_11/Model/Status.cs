using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project_11.Model
{
    public class Status : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int TotalMatch { get; set; }
        public int Win {  get; set; }
        public int Lose { get; set; }
        public double WinRate { get; set; }
        public int Rating { get; set; }
        public bool IsPlaying { get; set; } = false; // 게임 중인지 아닌지
        public string Color { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
