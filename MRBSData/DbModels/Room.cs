using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    [Table("Room")]
    public class Room
    {
        public int RoomId { get; set; }

        [Display(Name = "Room")]
        [StringLength(450)]
        [Index(IsUnique = true)]
        public string RoomName { get; set; }

        public string Description { get; set; }

        public int Capacity { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
