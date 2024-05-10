using System.ComponentModel.DataAnnotations;

namespace DotNetCourse.Models
{
    public class CommentModel
    {
        public int Id { get; set; }

        // Foreign key to the associated Blog entity
        public int BlogId { get; set; }

        // Foreign key to the associated User entity
        public int UserId { get; set; }
        

        [Required(ErrorMessage = "Text is required")]
        public string Text { get; set; }


        [Required(ErrorMessage = "Created by is required")]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation properties
        public UserModel User { get; set; } // Navigation property for the associated user
        public BlogModel Blog { get; set; } // Navigation property for the associated blog
    }


}
