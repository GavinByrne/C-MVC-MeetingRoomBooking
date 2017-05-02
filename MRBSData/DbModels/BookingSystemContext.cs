using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BookingSystemData.DbModels
{
    public class BookingSystemContext : DbContext
    {
        public BookingSystemContext() : base("DefaultConnection")
        {

        }

        public DbSet<BookingUser> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        //public System.Data.Entity.DbSet<MRBS.Models.BookingEdit> BookingEdits { get; set; }
    }
}
