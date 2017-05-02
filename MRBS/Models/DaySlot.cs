using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class DaySlot
    {
        //public int Day { get; set; }
        public ICollection<BookingSlot> Slots { get; set; }
    }
}