using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DotNetCourse.Models
{
    public class UserModel
    {
        // Additional properties for your custom user model

        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } 
        
        [Required(ErrorMessage = "Email is required")]
        public string Username { get; set; } 
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public ICollection<CommentModel> Comments { get; set; }
        public string? passwordResetToken { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set;}

        public ICollection<BlogModel> Blogs { get; set; }


        // Override properties to align with your database schema

    }

    public enum UserRole
    {
        Admin,
        Blogger,

    }


}
