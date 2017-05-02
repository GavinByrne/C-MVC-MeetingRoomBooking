using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRBS.Models
{
    public class BookingTable
    {
        public ICollection<BookingRow> Rows { get; set; }
        public DateTime Today { get; set; }

    }
}