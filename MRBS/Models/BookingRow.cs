using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class BookingRow
    {
        public ICollection<BookingSlot> Slots { get; set; }
        public ICollection<RoomHeader> Rooms { get; set; }
        public ICollection<WeeklyHeader> WeekDays { get; set; }
    }
}