using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project_11.Model
{
    public class Status
    {
        public string Name { get; set; }
        public int TotalMatch { get; set; }
        public int Win {  get; set; }
        public int Lose { get; set; }
        public double Rate { get; set; }
        public int Rating { get; set; }
    }
}
