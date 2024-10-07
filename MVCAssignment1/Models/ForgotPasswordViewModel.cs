using System.ComponentModel.DataAnnotations;

namespace MVCAssignment1.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string EmailAddress { get; set; }
    }
}
