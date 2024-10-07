using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCAssignment1.Models
{
    public class PasswordDetails
    {
       
        [Key, ForeignKey("UserDetail")]
        public int UserID { get; set; }
        public string Password { get; set; }

        public virtual UserDetail UserDetail { get; set; }
    }
}
