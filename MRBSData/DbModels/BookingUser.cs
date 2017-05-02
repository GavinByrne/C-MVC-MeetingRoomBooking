using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystemData.DbModels
{
    public class BookingUser
    {
        /// <summary>
        /// Usernames should be specified with Domain\Username, e.g. EUROPE\ReillyS2
        /// </summary>
        [Key]
        public string Login { get; set; }

        /// <summary>
        /// Roles should be seperated with ; e.g. admin; superuser
        /// </summary>
        public string Roles { get; set; }
    }
}
