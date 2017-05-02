using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class WeekDays
    {   
        public DateTime DateOfDay { get; set;  }
        public string DayOfWeek { get; set; }
        public int DayIndex { get; set; }
        public bool isSelected { get; set; }
    }
}