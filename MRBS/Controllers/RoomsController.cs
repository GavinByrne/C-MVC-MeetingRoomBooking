using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BookingSystemData.DbModels;
using PagedList;
using MRBS.Attributes;

namespace MRBS.Controllers
{
    public class RoomsController : Controller
    {
        private BookingSystemContext db = new BookingSystemContext();

        // GET: Room
        public ActionResult Index(string sortOrder, string option, string searchString, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.nameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.descrSortParm = sortOrder == "descr" ? "descr_desc" : "descr";
            ViewBag.capacitySortParm = sortOrder == "capacity" ? "capacity_desc" : "capacity";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var details = from r in db.Rooms select r;

            if (!String.IsNullOrEmpty(searchString))
            {
                details = details.Where(r => (r.RoomName.Contains(searchString)) || (r.Description.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    details = details.OrderByDescending(d => d.RoomName);
                    break;
                case "descr":
                    details = details.OrderBy(d => d.Description);
                    break;
                case "descr_desc":
                    details = details.OrderByDescending(d => d.Description);
                    break;
                case "capacity":
                    details = details.OrderBy(d => d.Capacity);
                    break;
                case "capacity_desc":
                    details = details.OrderByDescending(d => d.Capacity);
                    break;
                default:
                    details = details.OrderBy(d => d.RoomName);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            BookingRepository repository = new BookingRepository();
            var userRoles = repository.GetRoles(User.Identity.Name);

            if (userRoles.Any(ur => ur == "admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }

            return View(details.ToPagedList(pageNumber, pageSize));
            //return View(db.Rooms.ToList());
        }

        // GET: Room/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Room/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Room/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Rooms,RoomName,Description,Capacity")] Room room)
        {

            if (ModelState.IsValid)
            {
                db.Rooms.Add(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(room);
        }

        // GET: Room/Edit/5
        [BookingAuthorize(Roles ="admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Room/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RoomId,RoomName,Description,Capacity")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(room);
        }

        // GET: Room/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Rooms.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Room room = db.Rooms.Find(id);
            db.Rooms.Remove(room);
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
    }
}
