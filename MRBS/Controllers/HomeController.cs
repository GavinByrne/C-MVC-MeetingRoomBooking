using BookingSystemData.DbModels;
using Microsoft.Ajax.Utilities;
using MRBS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MRBS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DailyBookings()
        {
            using (var bookingRepository = new BookingRepository())
            {
                var mgmt = new BookingMgmt(bookingRepository);
                return View(mgmt.GetDailyBookings(DateTime.Now));
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}