using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    public class BookingRepository : IBookingRepository, IDisposable
    {
        private BookingSystemContext context;

        public BookingRepository ()
        {
            context = new BookingSystemContext();        
        }

        public IEnumerable<Booking> GetBookings()
        {
            return context.Bookings.ToList();
        }

        public Booking GetBookingByID(int? Id)
        {
            return context.Bookings.Find(Id);
        }

        public string[] GetRoles(string name)
        {
            var user = context.Users.FirstOrDefault(u => u.Login == name);

            if(user != null)
            {
                var roles = user.Roles.Split(new char[] { ';' } , StringSplitOptions.RemoveEmptyEntries);
                return roles;
            }

            return new string[] { };
        }

        public IEnumerable<Booking> GetBookingByDate(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

            return context.Bookings.Where(b => b.StartTime >= startOfDay && b.EndTime < endOfDay).ToList();
            //return context.Bookings.Find(date);
        }

        public IEnumerable<Booking> GetBookingsForPeriod(DateTime startDate, DateTime endDate, string roomName)
        {
            return context.Bookings.Where(b => b.Room.RoomName == roomName &&
                b.StartTime >= startDate && b.EndTime <= endDate).ToList();
        }

        public IEnumerable<Room> GetBookingRooms()
        {
            return context.Rooms.ToList();
        }

        public void InsertBooking(Booking booking)
        {
            context.Bookings.Add(booking);
        }

        //public void InsertBooking(BookingEdit booking)
        //{
        //    context.Bookings.Add(booking);
        //}

        public void DeleteBooking(int Id)
        {
            Booking booking = context.Bookings.Find(Id);
            context.Bookings.Remove(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            context.Entry(booking).State = EntityState.Modified;
        }



        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //public Booking GetBookingByID(int? id)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
