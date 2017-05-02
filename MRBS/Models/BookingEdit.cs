using BookingSystemData.DbModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MRBS.Models
{
    public class BookingEdit
    {
        [Key]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "A Title is required.")]
        [RegularExpression("^[a-zA-Z ]*$")]
        [StringLength(20, MinimumLength = 5)]
        public string Title { get; set; }

        [Required(ErrorMessage = "A Description is required.")]
        [StringLength(160, MinimumLength = 5)]
        public string Description { get; set; }


        [Display(Name = "Confirmation Status")]
        public bool Confirmation { get; set; }

        [Required]
        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime DateCreated { get; set; }

        public string BookingType { get; set; }
        [Display(Name = "Type")]
        public IEnumerable<SelectListItem> BookingTypeDropDown { get; set; }

        public IEnumerable<int> RoomIds { get; set; }
        [Display(Name = "Room")]
        public IEnumerable<SelectListItem> RoomsDropDown { get; set; }

        public string StartTime { get; set; }
        public IEnumerable<SelectListItem> StartTimeDropDown { get; set; }

        public string EndTime { get; set; }
        public IEnumerable<SelectListItem> EndTimeDropDown { get; set; }

        public string DayOfMonth { get; set; }
        public IEnumerable<SelectListItem> DayOfMonthDropDown { get; set; }

        public string MonthIncrement { get; set; }
        public IEnumerable<SelectListItem> MonthIncrementDropDown { get; set; }

        public string MonthDay { get; set; }
        public IEnumerable<SelectListItem> MonthDayDropDown { get; set; }

        public List<WeekDays> DayList { get; set; }

        //public int[] SelectedRoomId { get; set; }


        //hidden properties
        public string ViewName { get; set; }
        public string ViewRoomName { get; set; }

        //recurring properties
        //public bool RepeatDaily { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime RepeadEnd { get; set; }

        public Booking GetBooking(int id)
        {
            Booking booking = new Booking();
            booking.BookingId = this.BookingId;
            //booking.BookingType = this.BookingType.FirstOrDefault(b => b.Selected).Value;
            booking.BookingType = this.BookingType;
            booking.StartTime = this.StartDate;
            booking.EndTime = this.EndDate;
            booking.Confirmation = this.Confirmation;
            booking.CreatedBy = this.CreatedBy;
            booking.DateCreated = this.DateCreated;
            booking.Description = this.Description;
            //booking.RoomId = int.Parse(this.Rooms.FirstOrDefault(r => r.Selected).Value);

            //todo - we need some way to handle multiple room ids. For now just take the first one..

            //we also validation to prevent users from not selecting any rooms
            //GB: They can't do this. It's auto selected from the calender view and carried over to the create page.
            //booking.RoomId = this.RoomIds.First();
            booking.RoomId = id;
            booking.Title = this.Title;



            return booking;
            
        }

    }
}