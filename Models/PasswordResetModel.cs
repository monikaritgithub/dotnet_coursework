using System.ComponentModel.DataAnnotations;

namespace DotNetCourse.Models
{
    public class PasswordResetModel
    {
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
        public string Email { get; set; }
        public string token { get; set; }
    }
}
