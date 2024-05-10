using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DotNetCourse.Models
{
    public class BlogModel
    {
        
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [BindNever]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Short Description is required")]
        public string ShortDescription { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        public ICollection<CommentModel> Comments { get; set; }
       

        [Required(ErrorMessage = "Created by is required")]
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int Popularity { get; set; }
        // Foreign key property
        public int UserId { get; set; }
        public UserModel User { get; set; }
        // Constructor to initialize the CreatedAt property
       
    }
}
