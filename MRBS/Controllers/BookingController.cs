using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookingSystemData.DbModels;
using MRBS.Models;
using System.Configuration;
using AutoMapper;
using System.Text;

namespace MRBS.Controllers
{
    public class BookingController : Controller
    {
        private BookingSystemContext db = new BookingSystemContext();

        private IBookingRepository bookingRepository;

        public List<DateTime> potentialBookings = new List<DateTime>();

        public BookingController()
        {
            this.bookingRepository = new BookingRepository(/*new BookingSystemContext()*/);
        }

        public BookingController(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }

        // GET: Booking
        public ActionResult DailyBookings(DateTime? date = null)
        {
            //var bookings = db.Bookings.Include(b => b.Room);
            if (date == null)
            {
                date = DateTime.Today;
            }

            using (var bookingRepository = new BookingRepository())
            {
                var mgmt = new BookingMgmt(bookingRepository);
                return View(mgmt.GetDailyBookings(date.Value));
            }

        }

        public ActionResult WeeklyBookings(string roomName, DateTime? date)
        {
            if (date == null)
            {
                date = DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek) + 6) % 7);
            }

            if (roomName == null)
            {
                roomName = "Rush";
            }
            ViewBag.SelectRoom = roomName;

            using (var bookingRepository = new BookingRepository())
            {
                var mgmt = new BookingMgmt(bookingRepository);
                return View(mgmt.GetWeeklyBookings(date.Value, roomName));
            }
        }

        public ActionResult MonthlyBookings(DateTime? date, string roomName)
        {
            if (date == null)
            {
                date = DateTime.Today;
            }
            if (roomName == null)
            {
                roomName = "Rush";
            }
            ViewBag.SelectRoom = roomName;

            using (var bookingRepository = new BookingRepository())
            {
                var mgmt = new BookingMgmt(bookingRepository);
                return View(mgmt.GetMonthlyBookings(date.Value, roomName));
            }
        }

        // GET: Booking/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = bookingRepository.GetBookingByID(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            BookingRepository repository = new BookingRepository();
            var userRoles = repository.GetRoles(User.Identity.Name);
            if (booking.CreatedBy == User.Identity.Name || userRoles.Any(ur => ur == "admin"))
            {
                ViewBag.IsAuthorized = true;
            }
            else
            {
                ViewBag.IsAuthorized = false;
            }
            return View(booking);
        }

        // GET: Booking/Create
        [Authorize]
        public ActionResult Create(DateTime date, DateTime starttime1, DateTime endtime1, string roomname)
        {
            Uri myURl = Request.UrlReferrer;

            string view;

            if (myURl.AbsolutePath == "/")
            {
                view = myURl.AbsolutePath;
            }
            else
            {
                view = myURl.Segments[2];
            }

            var mgmt = new BookingMgmt(bookingRepository);
            return View(mgmt.GetBookingEdit(date, starttime1, endtime1, roomname, view));
        }


        // POST: Booking/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingEdit booking, string bookingType, string viewName, string viewRoomName, string option, string monthlyoption, int increment)
        {
            DateTime resetDate = booking.StartDate;
            booking.RepeadEnd = booking.RepeadEnd.AddDays(1).AddSeconds(-1);

            booking.DateCreated = DateTime.Now;
            booking.CreatedBy = User.Identity.Name.ToString();
            booking.StartDate = booking.StartDate.AddHours(double.Parse(booking.StartTime));
            booking.EndDate = booking.EndDate.AddHours(double.Parse(booking.EndTime));
            booking.BookingType = bookingType;

            var mgmt = new BookingMgmt(bookingRepository);

            switch (option)
            {
                case "Daily":
                    potentialBookings = mgmt.DailyRecurring(booking);
                    break;
                case "Weekly":
                    potentialBookings = mgmt.WeeklyRecurring(booking, increment);
                    break;
                case "Monthly":
                    if (monthlyoption == "Day")
                    {
                        potentialBookings = mgmt.MonthlyByDay(booking);
                    }
                    else if (monthlyoption == "Increment")
                    {
                        potentialBookings = mgmt.MonthlyIncrement(booking);
                    }
                    break;
                default: // code for non recurring
                    if (ModelState.IsValid)
                    {
                        bool conflict = false;

                        foreach (var id in booking.RoomIds)
                        {

                            if (db.Bookings.Any(b => ((b.StartTime >= booking.StartDate) && (b.EndTime <= booking.EndDate)) && b.RoomId.Equals(id)))
                            {
                                conflict = true;
                                ViewBag.Error = "This room and time have been reserved.";
                            }
                        }

                        if (!conflict)
                        {
                            if (booking.EndDate < booking.StartDate)
                            {
                                ViewBag.Error = "The End Date Can't Be Before the Start Date.";
                            }
                            else
                            {
                                foreach (var id in booking.RoomIds)
                                {
                                    bookingRepository.InsertBooking(booking.GetBooking(id));
                                    bookingRepository.Save();
                                }

                                if (viewName == "/")
                                {
                                    return RedirectToAction("DailyBookings", new { date = booking.StartDate.ToString("yyyy-MM-dd") });
                                }
                                else
                                {
                                    return RedirectToAction(viewName, new { date = booking.StartDate.AddDays(-(((int)booking.StartDate.DayOfWeek) + 6) % 7), roomName = viewRoomName });
                                }
                            }

                        }
                    }
                    return View(mgmt.GetBookingEdit(booking.StartDate, booking.StartDate, booking.EndDate, booking.ViewRoomName, booking.ViewName)); 
                    //break;
            }

            List<Booking> failedBookings = new List<Booking>();

            foreach (var date in potentialBookings)
            {
                booking.StartDate = date.AddHours(double.Parse(booking.StartTime));
                booking.EndDate = date.AddHours(double.Parse(booking.EndTime));

                if (ModelState.IsValid)
                {
                    foreach (var id in booking.RoomIds)
                    {
                        if (db.Bookings.Any(b => ((b.StartTime >= booking.StartDate) && (b.EndTime <= booking.EndDate)) && b.RoomId.Equals(id)))
                        {
                            
                            failedBookings.Add(booking.GetBooking(id));
                            //ViewBag.Error = "This room and time have been reserved.";
                            //potentialBookings.Remove(date);

                        }
                        else
                        {
                            bookingRepository.InsertBooking(booking.GetBooking(id));
                            bookingRepository.Save();
                        }
                    }
                }
            }
            StringBuilder sb = new StringBuilder();
            if (failedBookings.Count >= 1)
            {

                sb.Append("The following were not booked due to confilcts: \n\n");

                foreach (var item in failedBookings)
                {
                    string roomName = db.Rooms.
                                      Where(r => (r.RoomId.Equals(item.RoomId))).
                                      Select(r => r.RoomName).FirstOrDefault();

                    sb.Append(item.StartTime + " - " + item.EndTime.TimeOfDay + " "  + roomName + "\n");
                }

            }
            TempData["ErrorList"] = sb.ToString();

            if (viewName == "/")
            {
                return RedirectToAction("DailyBookings", new { date = resetDate.ToString("yyyy-MM-dd") });
            }
            else
            {
                return RedirectToAction(viewName, new { date = resetDate.AddDays(-(((int)resetDate.DayOfWeek) + 6) % 7), roomName = viewRoomName });
            }

            //var mgmt = new BookingMgmt(bookingRepository);
            //return View(mgmt.GetBookingEdit(booking.StartDate, booking.StartDate, booking.EndDate, booking.ViewRoomName, booking.ViewName));
        }

        // GET: Booking/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);

            var mgmt = new BookingMgmt(bookingRepository);

            BookingRepository repository = new BookingRepository();
            var userRoles = repository.GetRoles(User.Identity.Name);

            if (booking.CreatedBy == User.Identity.Name || userRoles.Any(ur => ur == "admin"))
            {
                return View(mgmt.GetEdit(booking));
            }
            else
            {
                return View("Unauthorized");
            }

            //if (booking == null)
            //{
            //    return HttpNotFound();
            //}
            //ViewBag.Rooms = new SelectList(db.Room, "Rooms", "RoomName", booking.Rooms);
            //return View(booking);
        }

        // POST: Booking/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookingEdit booking, string bookingType)
        {

            booking.DateCreated = DateTime.Now;
            booking.CreatedBy = User.Identity.Name.ToString();
            booking.StartDate = booking.StartDate.AddHours(double.Parse(booking.StartTime));
            booking.EndDate = booking.EndDate.AddHours(double.Parse(booking.EndTime));
            booking.BookingType = bookingType;



            if (ModelState.IsValid)
            {
                bool conflict = false;

                foreach (var id in booking.RoomIds)
                {
                    if (db.Bookings.Any(b => ((b.StartTime >= booking.StartDate) && (b.EndTime <= booking.EndDate)) && b.RoomId.Equals(id) && b.BookingId != booking.BookingId))
                    {
                        conflict = true;
                        ViewBag.Error = "This room and time have been reserved.";
                    }
                }
                if (!conflict)
                {
                    if (booking.EndDate < booking.StartDate)
                    {
                        ViewBag.Error = "The End Date Can't Be Before the Start Date.";
                    }
                    else
                    {
                        foreach (var id in booking.RoomIds)
                        {
                            db.Entry(booking.GetBooking(id)).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        return RedirectToAction("Details", new { id = booking.BookingId });
                    }
                }
            }
            var mgmt = new BookingMgmt(bookingRepository);
            return View(mgmt.GetBookingEdit(booking.StartDate, booking.StartDate, booking.EndDate, booking.ViewRoomName, booking.ViewName));
        }

        // GET: Booking/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            BookingRepository repository = new BookingRepository();
            var userRoles = repository.GetRoles(User.Identity.Name);

            if (booking.CreatedBy == User.Identity.Name || userRoles.Any(ur => ur == "admin"))
            {
                return View(booking);
            }
            else
            {
                return View("Unauthorized");
            }
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //public static string getBetween(string strSource, string strStart, string strEnd)
        //{
        //    int Start, End;
        //    if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        //    {
        //        Start = strSource.IndexOf(strStart, 0) + strStart.Length;
        //        End = strSource.IndexOf(strEnd, Start);
        //        return strSource.Substring(Start, End - Start);
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}
    }
}
