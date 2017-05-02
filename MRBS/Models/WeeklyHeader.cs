using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class WeeklyHeader
    {
        public string Weekday { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime DayDate { get; set; }
    }
}