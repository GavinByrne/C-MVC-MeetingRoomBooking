using BookingSystemData.DbModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MRBS.Models
{
    public class BookingMgmt
    {
        public IBookingRepository bookingRepository;

        public BookingMgmt(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }

        //creates table for daily bookings.
        public BookingTable GetDailyBookings(DateTime date)
        {
            IEnumerable<Room> bookingRooms = bookingRepository.GetBookingRooms();
            BookingTable bookingTable = new BookingTable();
            bookingTable.Rows = new List<BookingRow>();

            bookingTable.Today = date;

            DateTime currentTime = date.Date.AddHours(7);
            DateTime endTime = date.Date.AddHours(19);

            int rowIndex = 0;
            var skipEmptySlotIndexes = new List<SlotIndex>();

            while (currentTime < endTime)
            {
                BookingRow row = new BookingRow();
                row.Slots = new List<BookingSlot>();
                row.Rooms = new List<RoomHeader>();
                row.Slots.Add(new BookingSlot { RoomName = "Time", IsTime = true, StartTime = currentTime });

                int colIndex = 0;

                foreach (var room in bookingRooms)
                {
                    IEnumerable<Booking> bookingsByDate = bookingRepository.GetBookingByDate(date).Where(r => r.Room.RoomName.Equals(room.RoomName));

                    Booking currentBooking = bookingsByDate.FirstOrDefault(b => b.StartTime.Equals(currentTime));

                    AddBookingSlots(currentBooking, row, skipEmptySlotIndexes, rowIndex, colIndex, currentTime, date.Date, room.RoomName, true, bookingRooms, room);
                    colIndex++;

                }
                bookingTable.Rows.Add(row);

                currentTime = currentTime.AddMinutes(30);
                rowIndex++;
            }
            return bookingTable;
        }

        //creates table for weekly bookings
        public WeeklyBookingTable GetWeeklyBookings(DateTime weekStartDate, string roomName)
        {
            List<WeeklyHeader> weekDays = WeekDays(weekStartDate);

            WeeklyBookingTable bookingTable = new WeeklyBookingTable();
            bookingTable.Rooms = bookingRepository.GetBookingRooms();
            bookingTable.Rows = new List<BookingRow>();
            bookingTable.Today = weekStartDate;

            DateTime currentTime = weekStartDate.Date.AddHours(7);
            DateTime endTime = weekStartDate.Date.AddHours(19);

            DateTime startOfWeek = weekStartDate.Date; //Assume Monday was passed in
            DateTime endOfWeek = weekStartDate.Date.AddDays(7).AddSeconds(-1); //23.59 Sunday
            IEnumerable<Booking> weeklyRoomBookings = bookingRepository.GetBookingsForPeriod(startOfWeek, endOfWeek, roomName);

            int rowIndex = 0;
            var skipEmptySlotIndexes = new List<SlotIndex>();

            while (currentTime < endTime)
            //Main loop needs to go through all the times of the day -> Create a new row for each time of day
            {
                BookingRow row = new BookingRow();
                row.WeekDays = new List<WeeklyHeader>();
                row.Slots = new List<BookingSlot>();
                row.Slots.Add(new BookingSlot { RoomName = "Time", IsTime = true, StartTime = currentTime });

                int colIndex = 0;

                //Add Column Headers 
                foreach (var day in weekDays)
                {
                    row.WeekDays.Add(new WeeklyHeader { Weekday = day.Weekday, DayOfWeek = day.DayOfWeek });
                }

                //Add Content for each column
                for (var currentDay = startOfWeek; currentDay <= endOfWeek; currentDay = currentDay.AddDays(1))
                {

                    //at 7.30 and finishes on* Monday*at 18.30 we can't use it *alone*.

                    var currentBooking = weeklyRoomBookings.FirstOrDefault(b =>
                        b.StartTime == currentDay.Add(currentTime.TimeOfDay) &&
                        b.Room.RoomName == roomName);

                    AddBookingSlots(currentBooking, row, skipEmptySlotIndexes, rowIndex, colIndex, currentTime, currentDay, roomName, false, null, null);
                    colIndex++;
                }

                bookingTable.Rows.Add(row);
                currentTime = currentTime.AddMinutes(30);
                rowIndex++;
            }

            return bookingTable;
        }

        public MonthlyBookingTable GetMonthlyBookings(DateTime date, string roomName)
        {
            MonthlyBookingTable bookingTable = new MonthlyBookingTable();
            bookingTable.DaySlots = new List<DaySlot>();
            bookingTable.Rooms = bookingRepository.GetBookingRooms();

            bookingTable.Today = date;
            
            //int calendarIndex;
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            DateTime endOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1); //23.59 Sunday
            IEnumerable<Booking> MonthlyRoomBookings = bookingRepository.GetBookingsForPeriod(firstDayOfMonth, endOfMonth, roomName);

                for (var dateIndex = firstDayOfMonth; dateIndex <= endOfMonth; dateIndex = dateIndex.AddDays(1))
                {
                var daySlot = new DaySlot();
                daySlot.Slots = new List<BookingSlot>();

                 var currentBookings = MonthlyRoomBookings.Where(b =>
                  b.StartTime >= dateIndex &&
                  b.Room.RoomName == roomName);

                foreach (var booking in currentBookings)
                {
                    var durationTimeSpan = booking.EndTime - booking.StartTime;
                    var duration = (int)durationTimeSpan.TotalMinutes / 30;

                    //if (booking != null)
                    //{
                    daySlot.Slots.Add(new BookingSlot
                    {
                        CurrentDay = dateIndex,
                        BookingId = booking.BookingId,
                        RoomName = roomName,
                        IsTime = true,
                        IsBooked = true,
                        BookingType = booking.BookingType,
                        Duration = duration,
                        Title = booking.Title,
                        StartTime = booking.StartTime,
                        EndTime = booking.EndTime
                    });
                    //}
                    ////else
                    ////{
                    ////    daySlot.Slots.Add(new BookingSlot
                    ////    {
                    ////        CurrentDay = dateIndex,
                    ////        BookingId = booking.BookingId,
                    ////        RoomName = roomName,
                    ////        IsTime = false,
                    ////        IsBooked = false,
                    ////        BookingType = booking.BookingType,
                    ////        Duration = duration,
                    ////        Title = booking.Title,
                    ////        StartTime = booking.StartTime,
                    ////        EndTime = booking.EndTime
                    ////    });
                    ////}                 
                }
                bookingTable.DaySlots.Add(daySlot);
            }

            return bookingTable;
        }

        public void AddBookingSlots(Booking currentBooking, BookingRow row, List<SlotIndex> skipEmptySlotIndexes, int rowIndex, int colIndex, DateTime currentTime, DateTime currentDay, string roomName, bool isDaily, IEnumerable<Room> bookingRooms, Room room)
        {
            if (currentBooking != null)
            {
                var durationTimeSpan = currentBooking.EndTime - currentBooking.StartTime;
                var duration = (int)durationTimeSpan.TotalMinutes / 30;

                row.Slots.Add(new BookingSlot { BookingType = currentBooking.BookingType, BookingId = currentBooking.BookingId, RoomName = roomName, IsBooked = true, IsTime = false, Duration = duration, StartTime = currentBooking.StartTime, EndTime = currentBooking.EndTime, Title = currentBooking.Title });
                if (isDaily == true)
                {
                    row.Rooms.Add(new RoomHeader { RoomName = room.RoomName, Capacity = room.Capacity });
                }
                if (duration > 1)
                {
                    AddSkipLocations(skipEmptySlotIndexes, duration, rowIndex, colIndex);
                }
            }
            else if (skipEmptySlotIndexes.IsEmptySlotRequired(colIndex, rowIndex))
            {
                row.Slots.Add(new BookingSlot { CurrentDay = currentDay.Date, RoomName = roomName, IsBooked = false, IsTime = false, Duration = 1, StartTime = currentTime, EndTime = currentTime.AddMinutes(30) });
                if (isDaily == true)
                {
                    row.Rooms.Add(new RoomHeader { RoomName = room.RoomName, Capacity = room.Capacity });
                }
            }
        }

        private void AddSkipLocations(ICollection<SlotIndex> skipEmptySlotIndexes, int duration, int currentRow, int currentCol)
        {
            var emptySlotsToAdd = duration - 1;

            int i = 1;
            while (i <= emptySlotsToAdd)
            {
                skipEmptySlotIndexes.Add(new SlotIndex(currentCol, currentRow + i));
                i++;
            }
        }

        //private IEnumerable<SelectListItem> GetTimeDropDownList(TimeSpan selectedTime)
        //{

        //    var timeDropDownList = new List<SelectListItem>();

        //    var startOfDay = DateTime.ParseExact(ConfigurationManager.AppSettings["StartOfDay"], "HH:mm", CultureInfo.CurrentCulture);
        //    var endOfDay = DateTime.ParseExact(ConfigurationManager.AppSettings["EndOfDay"], "HH:mm", CultureInfo.CurrentCulture);
        //    double slotMins = double.Parse(ConfigurationManager.AppSettings["SlotDurationMins"]);

        //    for (DateTime i = startOfDay; i <= endOfDay; i = i.AddMinutes(slotMins))
        //    {
        //        var isSelected = (selectedTime.Hours.Equals(i.Hour) && (selectedTime.Minutes.Equals(i.Minute)));

        //        timeDropDownList.Add(new SelectListItem { Text = i.ToString("HH:mm"), Value = (((double)(i.Hour) + ((double)(i.Minute) / 60)).ToString()/*.PadLeft(2, '0')*/), Selected = isSelected });

        //    }

        //    return timeDropDownList;
        //}

        private IEnumerable<SelectListItem> GetTimeDropDownList(TimeSpan startTime)
        {

            var timeDropDownList = new List<SelectListItem>();


            
            //var startOfDay = DateTime.ParseExact(ConfigurationManager.AppSettings["StartOfDay"], "HH:mm", CultureInfo.CurrentCulture);
            var endOfDay = TimeSpan.ParseExact(ConfigurationManager.AppSettings["EndOfDay"], @"hh\:mm", CultureInfo.CurrentCulture);
            var slotMins = TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["SlotDurationMins"]));

            for (TimeSpan i = startTime; i <= endOfDay; i = i.Add(slotMins))
            {
                var isSelected = (startTime.Hours.Equals(i.Hours) && (startTime.Minutes.Equals(i.Minutes)));

                timeDropDownList.Add(new SelectListItem { Text = i.ToString(@"hh\:mm"), Value = (((double)(i.Hours) + ((double)(i.Minutes) / 60)).ToString()/*.PadLeft(2, '0')*/), Selected = isSelected });

            }

            return timeDropDownList;
        }

        private IEnumerable<SelectListItem> GetDayOfMonthDropDown()
        {

            var dayDropDownList = new List<SelectListItem>();

            for (int i = 1; i < 32; i++)
            {
                dayDropDownList.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            return dayDropDownList;
        }

        private IEnumerable<SelectListItem> GetMonthlyIncrement()
        {

            var incrementDownList = new List<SelectListItem>();

            incrementDownList.Add(new SelectListItem { Text = "first", Value = "1", Selected = true });
            incrementDownList.Add(new SelectListItem { Text = "second", Value = "2"});
            incrementDownList.Add(new SelectListItem { Text = "third", Value = "3" });
            incrementDownList.Add(new SelectListItem { Text = "fourth", Value = "4"});
            incrementDownList.Add(new SelectListItem { Text = "last", Value = "5" });

            return incrementDownList;
        }

        private IEnumerable<SelectListItem> GetMonthlyDayList()
        {

            var dayDropDownList = new List<SelectListItem>();


            dayDropDownList.Add(new SelectListItem { Text = "Monday", Value = "Monday", Selected = true });
            dayDropDownList.Add(new SelectListItem { Text = "Tuesday", Value = "Tuesday" });
            dayDropDownList.Add(new SelectListItem { Text = "Wednesday", Value = "Wednesday" });
            dayDropDownList.Add(new SelectListItem { Text = "Thursday", Value = "Thursday" });
            dayDropDownList.Add(new SelectListItem { Text = "Friday", Value = "Friday" });
            dayDropDownList.Add(new SelectListItem { Text = "Saturday", Value = "Saturday" });
            dayDropDownList.Add(new SelectListItem { Text = "Sunday", Value = "Sunday" });

            return dayDropDownList;
        }

        private IEnumerable<SelectListItem> GetRooms(string roomName)
        {
            var roomMultiList = new List<SelectListItem>();

            IEnumerable<Room> rooms = bookingRepository.GetBookingRooms();

            foreach (var room in rooms)
            {
                var isSelected = room.RoomName.Equals(roomName);
                roomMultiList.Add(new SelectListItem { Value = room.RoomId.ToString(), Text = room.RoomName, Selected = isSelected });
            }

            return roomMultiList;
        }

        private IEnumerable<SelectListItem> GetRooms(int roomId)
        {
            var roomMultiList = new List<SelectListItem>();

            IEnumerable<Room> rooms = bookingRepository.GetBookingRooms();

            foreach (var room in rooms)
            {
                var isSelected = room.RoomId.Equals(roomId);
                roomMultiList.Add(new SelectListItem { Value = room.RoomId.ToString(), Text = room.RoomName, Selected = isSelected });
            }

            return roomMultiList;
        }

        private IEnumerable<SelectListItem> GetBookingType()
        {
            var typeList = new List<SelectListItem>();

            typeList.Add(new SelectListItem { Text = "Internal", Value = "Internal", Selected = true });
            typeList.Add(new SelectListItem { Text = "External", Value = "External" });

            return typeList;

        }

        private IEnumerable<SelectListItem> GetBookingType(string type)
        {
            var typeList = new List<SelectListItem>();


            if (type.Equals("Internal"))
            {
                typeList.Add(new SelectListItem { Text = "Internal", Value = "Internal", Selected = true });
                typeList.Add(new SelectListItem { Text = "External", Value = "External" });
            }
            else
            {
                typeList.Add(new SelectListItem { Text = "Internal", Value = "Internal"});
                typeList.Add(new SelectListItem { Text = "External", Value = "External", Selected = true});
            }


            return typeList;

        }

        private List<WeekDays> GetWeekDays(DateTime selectedDate)
        {
            //var monthStartIndex = (((int)firstDayOfMonth.DayOfWeek) + 6) % 7;

            var test = ((int)selectedDate.DayOfWeek);

            DateTime firstDateOfWeek;

            if (((int)selectedDate.DayOfWeek) == 1)
            {
                firstDateOfWeek = selectedDate;
            }
            else if (((int)selectedDate.DayOfWeek) == 2)
            {
                firstDateOfWeek = selectedDate.AddDays(-1);
            }
            else if (((int)selectedDate.DayOfWeek) == 3)
            {
                firstDateOfWeek = selectedDate.AddDays(-2);
            }
            else if (((int)selectedDate.DayOfWeek) == 4)
            {
                firstDateOfWeek = selectedDate.AddDays(-3);
            }
            else if (((int)selectedDate.DayOfWeek) == 5)
            {
                firstDateOfWeek = selectedDate.AddDays(-4);
            }
            else if (((int)selectedDate.DayOfWeek) == 7)
            {
                firstDateOfWeek = selectedDate.AddDays(-5);
            }
            else
            {
                firstDateOfWeek = selectedDate.AddDays(-6);
            }



            var dayList = new List<WeekDays>();

            for (int i = 0; i < 7; i++)
            {
                bool selected = selectedDate == firstDateOfWeek.AddDays(i);
                dayList.Add(new WeekDays {DateOfDay = firstDateOfWeek.Date.AddDays(i), DayOfWeek = firstDateOfWeek.AddDays(i).DayOfWeek.ToString(), DayIndex = ((int)firstDateOfWeek.AddDays(i).DayOfWeek), isSelected = selected });
            }

            //dayList.Add(new WeekDays { DayOfWeek = "Monday", DayIndex = 1 });
            //dayList.Add(new WeekDays { DayOfWeek = "Tuesday", DayIndex = 2 });
            //dayList.Add(new WeekDays { DayOfWeek = "Wednesday", DayIndex = 3 });
            //dayList.Add(new WeekDays { DayOfWeek = "Thursday", DayIndex = 4 });
            //dayList.Add(new WeekDays { DayOfWeek = "Friday", DayIndex = 5 });
            //dayList.Add(new WeekDays { DayOfWeek = "Saturday", DayIndex = 6 });
            //dayList.Add(new WeekDays { DayOfWeek = "Sunday", DayIndex = 7 });

            return dayList;
        }

        public BookingEdit GetBookingEdit(DateTime date, DateTime starttime1, DateTime endtime1, string roomname, string viewName)
        {
            var bookingEdit = new BookingEdit();

            var start = starttime1.TimeOfDay;
            var slotDurationMins = int.Parse(ConfigurationManager.AppSettings["SlotDurationMins"]);
            string stime = starttime1.TimeOfDay.ToString();

            bookingEdit.BookingTypeDropDown = GetBookingType();
            bookingEdit.StartTimeDropDown = GetTimeDropDownList(start);
            bookingEdit.EndTimeDropDown = GetTimeDropDownList(start.Add(new TimeSpan(0, slotDurationMins, 0)));
            bookingEdit.RoomsDropDown = GetRooms(roomname);
            bookingEdit.DateCreated = DateTime.Now;
            bookingEdit.StartDate = date;
            bookingEdit.EndDate = date;
            bookingEdit.RepeadEnd = date;
            bookingEdit.DayList = GetWeekDays(date);
            bookingEdit.DayOfMonthDropDown = GetDayOfMonthDropDown();
            bookingEdit.MonthIncrementDropDown = GetMonthlyIncrement();
            bookingEdit.MonthDayDropDown = GetMonthlyDayList();


            //hidden properties
            bookingEdit.ViewName = viewName;
            bookingEdit.ViewRoomName = roomname;

            return bookingEdit;

        }

        public BookingEdit GetEdit (Booking booking)
        {
            var bookingEdit = new BookingEdit();

            //var start = starttime1.TimeOfDay;
            var start = booking.StartTime.TimeOfDay; ;
            var slotDurationMins = int.Parse(ConfigurationManager.AppSettings["SlotDurationMins"]);
            //string stime = starttime1.TimeOfDay.ToString();

            bookingEdit.BookingId = booking.BookingId;
            bookingEdit.BookingTypeDropDown = GetBookingType(booking.BookingType.ToString());
            bookingEdit.StartTimeDropDown = GetTimeDropDownList(start);
            bookingEdit.EndTimeDropDown = GetTimeDropDownList(start.Add(new TimeSpan(0, slotDurationMins, 0)));
            bookingEdit.RoomsDropDown = GetRooms(booking.RoomId);
            bookingEdit.DateCreated = booking.DateCreated; ;
            bookingEdit.StartDate = booking.StartTime;
            bookingEdit.EndDate = booking.EndTime;
            bookingEdit.Title = booking.Title;
            bookingEdit.Description = booking.Description;
            bookingEdit.Confirmation = booking.Confirmation;
            

            return bookingEdit;
        }

        public List<WeeklyHeader> WeekDays(DateTime date)
        {
            List<WeeklyHeader> weekDays = new List<WeeklyHeader>();
            var currentday = date.DayOfWeek.ToString();
            var firstDayOfWeekOffset = ((int)date.DayOfWeek + 6) % 7;
            var firstDayOfWeek = date.AddDays(-firstDayOfWeekOffset);

            for (int i = 0; i < 7; i++)
            {
                var day = firstDayOfWeek.AddDays(i);
                weekDays.Add(new WeeklyHeader { Weekday = day.ToString("ddd").Substring(0, 3), DayOfWeek = day.ToString("dd"), DayDate = day.Date });
            }
            return weekDays;
        }

        public List<DateTime> MonthlyByDay(BookingEdit booking)
        {
            int day = int.Parse(booking.DayOfMonth);

            //DateTime firstbooking = new DateTime(booking.StartDate.Year, booking.StartDate.Month, day);

            List<DateTime> bookingDates = new List<DateTime>();

            bookingDates.Add(booking.StartDate.Date);

            if (day >= 1 && day <= 28)
            {
                DateTime firstbooking = new DateTime(booking.StartDate.Year, booking.StartDate.Month, day);

                for (DateTime dt = firstbooking; dt < booking.RepeadEnd; dt = dt.AddMonths(1))
                {
                    bookingDates.Add(dt);
                }
            }
            else
            {
                DateTime firstbooking = new DateTime(booking.StartDate.Year, booking.StartDate.Month, 1);

                for (DateTime dt = firstbooking; dt < booking.RepeadEnd; dt = dt.AddMonths(1))
                {
                    int daysinmonth = DateTime.DaysInMonth(dt.Year, dt.Month);

                    if (daysinmonth >= day)
                    {
                        DateTime newbookingdate = new DateTime(dt.Year, dt.Month, day);

                        bookingDates.Add(newbookingdate);
                    }
                    else
                    {
                        DateTime endofmonth = new DateTime(dt.Year, dt.Month, daysinmonth);

                        bookingDates.Add(endofmonth);
                    }

                }
            }
            return bookingDates;
        }

        public List<DateTime> MonthlyIncrement(BookingEdit booking)
        {
            List<DateTime> bookingDates = new List<DateTime>();

            bookingDates.Add(booking.StartDate.Date);

            if (int.Parse(booking.MonthIncrement) <= 4)
            {
                int count = 0;

                DateTime firstbooking = new DateTime(booking.StartDate.Year, booking.StartDate.Month, booking.StartDate.Day);

                for (DateTime dt = firstbooking; dt < booking.RepeadEnd; dt = dt.AddMonths(1))
                {
                    count = 0;
                    int daysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);

                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        DateTime currentDateTime = new DateTime(dt.Year, dt.Month, day);
                        if (currentDateTime.DayOfWeek.ToString() == booking.MonthDay)
                        {
                            count++;

                            if (count == int.Parse(booking.MonthIncrement))
                            {
                                bookingDates.Add(currentDateTime);
                                break;
                            }
                        }
                    }
                }
            }

            else
            {
                DateTime firstbooking = new DateTime(booking.StartDate.Year, booking.StartDate.Month, booking.StartDate.Day);

                for (DateTime dt = firstbooking; dt < booking.RepeadEnd; dt = dt.AddMonths(1))
                {

                    int daysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);

                    for (int day = daysInMonth; day > 0; day--)
                    {
                        DateTime currentDateTime = new DateTime(dt.Year, dt.Month, day);
                        if (currentDateTime.DayOfWeek.ToString() == booking.MonthDay)
                        {
                            bookingDates.Add(currentDateTime);
                            break;
                        }
                    }
                }
            }
            return bookingDates;
        }

        public List<DateTime> DailyRecurring(BookingEdit booking)
        {
            List<DateTime> bookingDates = new List<DateTime>();

            //bookingDates.Add(booking.StartDate.Date);

            while (booking.StartDate < booking.RepeadEnd)
            {
                bookingDates.Add(booking.StartDate.Date);

                booking.StartDate = booking.StartDate.AddDays(1);
            }
            return bookingDates;
        }

        public List<DateTime> WeeklyRecurring(BookingEdit booking, int increment)
        {
            int weekIncrement = 7 * increment;
            DateTime recurringStart = booking.StartDate.Date;

            List<DateTime> bookingDates = new List<DateTime>();
            bookingDates.Add(booking.StartDate.Date);

            foreach (var date in booking.DayList)
            {
                if (date.isSelected)
                {
                    if (date.DateOfDay < recurringStart) /*&& (date.DateOfDay <= booking.RepeadEnd)*/
                    {
                        date.DateOfDay = date.DateOfDay.AddDays(weekIncrement);
                    }
                    for (DateTime rDate = date.DateOfDay; rDate <= booking.RepeadEnd; rDate = rDate.AddDays(weekIncrement))
                    {
                        bookingDates.Add(rDate);
                    }
                }
            }
            return bookingDates;
        }
            
    }
}