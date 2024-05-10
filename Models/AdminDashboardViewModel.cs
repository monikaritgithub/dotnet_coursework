using System.Collections.Generic;

namespace DotNetCourse.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalBlogs { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int TotalComments { get; set; }
        public List<BlogViewModel> PostDetails { get; set; }
        public List<TopBloggerViewModel> TopBloggers { get; set; }
    }

    // ViewModel to hold details of each post
    public class BlogViewModel
    {
        public string BlogTitle { get; set; }
        public string ImagePath { get; set; }
        public int TotalComments { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
        public int Popularity { get; set; }
    }

    // ViewModel to hold details of top bloggers
    public class TopBloggerViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int TotalPopularity { get; set; }
    }
}
