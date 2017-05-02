using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    [Table("Bookings")]
    public class Booking
    {
        public int BookingId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool Confirmation { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string BookingType { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DateCreated { get; set; }

        public int RoomId { get; set; }

        public virtual Room Room { get; set; }


        public int[] SelectedRoomId { get; set; }
    }
}
