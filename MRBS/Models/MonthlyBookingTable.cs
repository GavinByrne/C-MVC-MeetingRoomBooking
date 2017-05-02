using BookingSystemData.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class MonthlyBookingTable
    {
        public int Day { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
        public IList<DaySlot> DaySlots { get; set; }
        public DateTime Today { get; set; }
    }
}