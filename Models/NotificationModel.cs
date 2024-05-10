using System;

namespace DotNetCourse.Models
{
    public class AlertModel
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; } // Foreign key to the associated BlogModel
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool isRead { get; set; }
        
  

        public AlertModel()
        {
            CreatedAt = DateTime.Now;
        }
    }
}
