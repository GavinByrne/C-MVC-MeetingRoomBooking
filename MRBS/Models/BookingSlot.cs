using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class BookingSlot
    {
        public DateTime CurrentDay { get; set; }
        public int BookingId { get; set; }
        public string RoomName { get; set; }
        public bool IsTime { get; set; }
        public bool IsBooked { get; set; }
        public string BookingType { get; set; }
        public int Duration { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }                            
}