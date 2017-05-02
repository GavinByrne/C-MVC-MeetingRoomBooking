using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookingSystemData.DbModels;

namespace MRBS.Models
{
    public class WeeklyBookingTable : BookingTable
    {
        public IEnumerable<Room> Rooms { get; set; }
    }
}