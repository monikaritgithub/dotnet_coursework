using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotNetCourse.Data;
using DotNetCourse.Models;

namespace DotNetCourse.Controllers
{
    public class AdminController : Controller
    {
        private readonly DotNetCourseDbContext _context;

        public AdminController(DotNetCourseDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index(string filterType = "all")
        {
            // Get total number of blogs
            var totalBlogs = await _context.Blogs.CountAsync();

            // Get total number of Likes and downvotes
            var totalLikes = _context.Blogs
    .Join(_context.Ranking,
        blog => blog.Id,
        voting => voting.TypeId,
        (blog, voting) => new { Blog = blog, Ranking = voting })
    .Where(x => x.Ranking.Type == "blog")
    .Sum(v => v.Ranking.Like);
            var totalDislikes =  _context.Blogs
    .Join(_context.Ranking,
        blog => blog.Id,
        voting => voting.TypeId,
        (blog, voting) => new { Blog = blog, Ranking = voting })
    .Where(x => x.Ranking.Type == "blog")
    .Sum(v => v.Ranking.Dislike);

            // Get total number of comments
            var totalComments = await _context.Comments.CountAsync();

            // Get details of each post
            // Filter blogs by month if specified
            var blogsQuery = _context.Blogs.AsQueryable();
            if (!string.IsNullOrEmpty(filterType))
            {
                if (filterType == "thisMonth")
                {
                    // Filter blogs by their creation date for this month
                    var currentMonth = DateTime.Today.Month;
                    var currentYear = DateTime.Today.Year;
                    blogsQuery = blogsQuery.Where(blog =>
                        blog.CreatedAt.Year == currentYear &&
                        blog.CreatedAt.Month == currentMonth
                    );
                }
                else if (filterType == "all")
                {
                    // No additional filtering required for "all" filter type
                }
                else
                {
                    // Attempt to parse the filterType as a month name or a numeric month
                    filterType = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(filterType.ToLower());
                    if (DateTime.TryParseExact(filterType, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMonth))
                    {
                        // Filter blogs by their creation date for the specified month
                        blogsQuery = blogsQuery.Where(blog =>
                            blog.CreatedAt.Year == DateTime.Today.Year &&
                            blog.CreatedAt.Month == parsedMonth.Month
                        );
                    }
                    else if (int.TryParse(filterType, out int month))
                    {
                        // Filter blogs by their creation date for the specified month
                        blogsQuery = blogsQuery.Where(blog =>
                            blog.CreatedAt.Year == DateTime.Today.Year &&
                            blog.CreatedAt.Month == month
                        );
                    }
                }
            }

            // Get details of top 10 blogs
            var blogDetails = await blogsQuery
                .Select(blog => new BlogViewModel
                {
                    BlogTitle = blog.Title,
                    ImagePath = blog.ImagePath,
                    TotalComments = _context.Comments.Count(c => c.BlogId == blog.Id),
                    TotalLikes = _context.Ranking.Where(v => v.Type == "blog" && v.TypeId == blog.Id).Sum(v => v.Like),
                    TotalDislikes = _context.Ranking.Where(v => v.Type == "blog" && v.TypeId == blog.Id).Sum(v => v.Dislike),
                    Popularity = _context.Ranking.Where(v => v.TypeId == blog.Id).Sum(v => v.Popularity)
                })
                .OrderByDescending(blog => blog.Popularity)
                .Take(10) // Limit to top 10 blogs
                .ToListAsync();


            // Filter bloggers by month if specified
            var bloggersQuery = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(filterType))
            {
                if (filterType == "thisMonth")
                {
                    // Filter bloggers by their blogs' creation date for this month
                    var currentMonth = DateTime.Today.Month;
                    var currentYear = DateTime.Today.Year;
                    bloggersQuery = bloggersQuery.Where(user =>
                        user.Role != UserRole.Admin &&
                        user.Blogs.Any(blog =>
                            blog.CreatedAt.Year == currentYear &&
                            blog.CreatedAt.Month == currentMonth
                        )
                    );
                }
                else if (filterType == "all")
                {
                    bloggersQuery = bloggersQuery.Where(user =>
                       user.Role != UserRole.Admin 
                   );
                }
                else
                {
                    // Attempt to parse the filterType as a month name
                     filterType = char.ToUpper(filterType[0]) + filterType.Substring(1);
                    var culture = CultureInfo.CurrentCulture;
                    var monthNames = culture.DateTimeFormat.MonthNames;
                    var monthIndex = Array.IndexOf(monthNames, filterType);
                    if (monthIndex != -1)
                    {
                        // Filter bloggers by their blogs' creation date for the specified month
                        bloggersQuery = bloggersQuery.Where(user =>
                            user.Role != UserRole.Admin &&
                            user.Blogs.Any(blog =>
                                blog.CreatedAt.Year == DateTime.Today.Year &&
                                blog.CreatedAt.Month == monthIndex + 1
                            )
                        );
                    }
                    else
                    {
                        // Attempt to parse the filterType as a numeric month
                        if (int.TryParse(filterType, out int month))
                        {
                            // Filter bloggers by their blogs' creation date for the specified month
                            bloggersQuery = bloggersQuery.Where(user =>
                                user.Role != UserRole.Admin &&
                                user.Blogs.Any(blog =>
                                    blog.CreatedAt.Year == DateTime.Today.Year &&
                                    blog.CreatedAt.Month == month
                                )
                            );
                        }
                    }
                }
            }

            // Get details of top 10 bloggers
            var topBloggers = await bloggersQuery
                .Select(user => new TopBloggerViewModel
                {
                    Username = user.Username,
                    Email = user.Email,
                    TotalPopularity = _context.Blogs.Where(blog => blog.UserId == user.Id).Sum(blog => blog.Popularity)
                })
                .OrderByDescending(user => user.TotalPopularity)
                .Take(10) // Limit to top 10 bloggers
                .ToListAsync();

            // Construct a ViewModel to hold the data
            var viewModel = new AdminDashboardViewModel
            {
                TotalBlogs = totalBlogs,
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
                TotalComments = totalComments,
                PostDetails = blogDetails,
                TopBloggers = topBloggers
            };

            return View("Admin", viewModel);
        }
    }
}
