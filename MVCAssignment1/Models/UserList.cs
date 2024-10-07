using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCAssignment1.Models
{
    public class UserList
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
    }
}