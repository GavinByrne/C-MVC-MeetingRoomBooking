using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    public interface IBookingRepository : IDisposable
    {
        IEnumerable<Booking> GetBookings();
        Booking GetBookingByID(int? Id);
        void InsertBooking(Booking booking);
        void DeleteBooking(int bookingId);
        void UpdateBooking(Booking booking);
        void Save();
        //Booking GetBookingByID(int? id);
        IEnumerable<Booking> GetBookingByDate(DateTime date);
        IEnumerable<Booking> GetBookingsForPeriod(DateTime startDate, DateTime endDate, string roomName);

        IEnumerable<Room> GetBookingRooms();

        string[] GetRoles(string name);
    }
}
