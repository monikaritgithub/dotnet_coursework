namespace DotNetCourse.Models
{
    public class UserAlert
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to the associated UserModel
        public int AlertId { get; set; } // Foreign key to the associated AlertModel
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public UserModel User { get; set; } // Reference to the associated user
        public AlertModel Alert { get; set; }
    }
}
