using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    public class BookingSystemInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<BookingSystemContext>
    {
        protected override void Seed(BookingSystemContext context)
        {
            SeedData(context);
        }

        //publicly accessible method for use by Configuration.cs
        public static void SeedData(BookingSystemContext context)
        {
            var date = DateTime.Today;

            var rooms = new List<Room>
            {
                new Room {RoomId=1, RoomName = "Garristown (GFEW)", Description = "Video Conf. Ground Floor East Wing",Capacity = 12, },
                new Room {RoomId=2, RoomName = "John F Walsh", Description = "Video Conf. Ground Floor North Wing",Capacity = 12, },
                new Room {RoomId=3, RoomName = "Lusk", Description = "1st Floor North Wing",Capacity = 4, },
                new Room {RoomId=4, RoomName = "Malahide", Description = "Ground Floor West Wing",Capacity = 14, },
                new Room {RoomId=5, RoomName = "Rush", Description = "1st Floor North Wing",Capacity = 4, },
                new Room {RoomId=6, RoomName = "Skerries (ex-RB)", Description = "1st Floor West Wing",Capacity = 5, },
                new Room {RoomId=7, RoomName = "Sutton (ex-OMK)", Description = "First Floor West Wing",Capacity = 5, },
                new Room {RoomId=8, RoomName = "Swords (ex-CD)", Description = "1st Floor West Wing",Capacity = 5, },
                new Room {RoomId=9, RoomName = "The Innbox", Description = "Innovation room",Capacity = 5, }
            };
            rooms.ForEach(r => context.Rooms.AddOrUpdate(r));
            context.SaveChanges();

            var bookings = new List<Booking>
            {
                 new Booking {RoomId= rooms.Single(r => r.RoomName == "Rush").RoomId, Description="Booked for 88 mins", Confirmation=true, StartTime= date.AddHours(7.5), EndTime= date.AddHours(8), BookingType="Internal", CreatedBy="Barru", DateCreated=DateTime.Now, Title= "Title"},
                 new Booking {RoomId= rooms.Single(r => r.RoomName == "Malahide").RoomId, Description="Booked for 60 mins", Confirmation=false,StartTime= date.AddHours(10), EndTime= date.AddHours(10.5), BookingType="Externnal", CreatedBy="Barru", DateCreated=DateTime.Now, Title= "Title1" },
                 new Booking {RoomId= rooms.Single(r => r.RoomName == "Rush").RoomId, Description="Booked for 30 mins", Confirmation=true, StartTime= date.AddHours(13), EndTime= date.AddHours(13.5), BookingType="Internal", CreatedBy="Barru", DateCreated=DateTime.Now, Title= "Title2" },
                 new Booking {RoomId= rooms.Single(r => r.RoomName == "Rush").RoomId, Description="Booked for 1000 mins", Confirmation=true, StartTime= date.AddHours(17), EndTime= date.AddHours(17.5), BookingType="Internal", CreatedBy="Barru", DateCreated=DateTime.Now, Title= "Title3"}
            };
            bookings.ForEach(b => context.Bookings.AddOrUpdate(b));
            context.SaveChanges();
        }
    }
}
