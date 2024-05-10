using System;
using System.IO;
using System.Threading.Tasks;
using DotNetCourse.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection.Metadata;
using DotNetCourse.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Microsoft.EntityFrameworkCore;


public class BlogController : Controller
{
    private readonly DotNetCourseDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public BlogController(DotNetCourseDbContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }


    public IActionResult Index(string sortOrder = "random",int page = 1, int pageSize = 10)
    {
        // Retrieve all blog posts
            var blogs = _context.Blogs.AsQueryable();
        ViewBag.SortOrder = sortOrder;

        switch (sortOrder)
        {
            case "date":
                blogs = blogs.OrderByDescending(b => b.CreatedAt);
                break;
            case "popularity":
                blogs = blogs.OrderByDescending(b => b.Popularity);
                break;
            default:
                Random rand = new Random();
                if (_context.Blogs.Count() == 0)
                {
                    IEnumerable<BlogMetaData> data = new List<BlogMetaData>();
                    return View(data);
                }
                int toSkip = rand.Next(1, _context.Blogs.Count());
                blogs = _context.Blogs.OrderBy(x => Guid.NewGuid());
                
                break;
        }
        
        var blogPosts = blogs.ToList();

        // Calculate the comments count for each blog post
        _context.SaveChanges();
        
        var blogsWithCommentsCount = blogPosts.Select(b => new BlogMetaData
        {
            Blog = b,
            CommentsCount = _context.Comments.Count(c => c.BlogId == b.Id),
            Like = _context.Ranking
          .Where(v => v.TypeId == b.Id && v.Type == "blog")
          .Sum(v => v.Like),
            Dislike = _context.Ranking
          .Where(v => v.TypeId == b.Id && v.Type == "blog")
          .Sum(v => v.Dislike)
        }).ToList();
        int totalItems = _context.Blogs.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // Get the subset of blog posts for the current page
        var currentPagePosts = blogsWithCommentsCount.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Pass the subset of blog posts and pagination information to the view
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.SortOrder = sortOrder;

        // Pass the blogs with comments count to the view
        return View(currentPagePosts);
    }

    public IActionResult ManageBlogs()
    {
        
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return Redirect("/user/login");
        var blogs = _context.Blogs.Where(b => b.UserId == int.Parse(userId)).ToList();
        return View(blogs);
    }
    public IActionResult GetBlog(int id)
    {
        var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);
        if (blog == null)
        {
            return NotFound();
        }

        return Json(blog);
    }
    // POST: /Blog/Create
    [HttpPost]
    public async Task<IActionResult> Create(BlogModel model, IFormFile image)
    {
        ModelState.Remove("ImagePath");
        ModelState.Remove("User");
        ModelState.Remove("Comments");
        if (ModelState.IsValid)
        {
      
           
            

            var compressedImage = await TinyImageAsync(image);
            var imagePath = await AddImageAsync(compressedImage);
            
            

            var blogPost = new BlogModel
            {
                Title = model.Title,
                ShortDescription = model.ShortDescription,
                Description = model.Description,
                CreatedBy = model.CreatedBy,
                ImagePath = imagePath,
                UserId = int.Parse(HttpContext.Session.GetString("UserId")),
                CreatedAt = DateTime.Now
            };

            _context.Blogs.Add(blogPost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Redirect to blog listing page
        }
        return View("ManageBlogs");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, BlogModel model, IFormFile image)
    {
        try
        {

            var existingBlogPost = await _context.Blogs.FindAsync(id); // Find the existing blog post by ID
            if (existingBlogPost == null)
            {
                return NotFound(); // Return not found if the blog post with the given ID doesn't exist
            }

            // Handle file upload if a new image is provided
            if (image != null && image.Length > 0)
            {
            var compressedImage = await TinyImageAsync(image);
            existingBlogPost.ImagePath = await AddImageAsync(compressedImage);
        }

            // Update other properties
            existingBlogPost.Title = model.Title;
            existingBlogPost.ShortDescription = model.ShortDescription;
            existingBlogPost.Description = model.Description;
            
            existingBlogPost.LastUpdatedAt = DateTime.Now;

            _context.Blogs.Update(existingBlogPost); // Update the existing blog post entity
            await _context.SaveChangesAsync();

            

        return RedirectToAction("ManageBlogs"); // Return to the index view if model state is not valid
    }catch(Exception e)
        {
            return null;
        }
        }



    // Method to handle file upload
    private async Task<string> AddImageAsync(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            return null;
        }

        var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
        var uniqueFileName = Guid.NewGuid().ToString() + "_image.jpg"; // Change the extension if needed
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        await System.IO.File.WriteAllBytesAsync(filePath, imageData);

        return "/uploads/" + uniqueFileName; // Return the relative path to the uploaded image
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var blogPost = await _context.Blogs.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        _context.Blogs.Remove(blogPost);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index)); // Redirect to the blog listing page
    }
    [HttpPost]
    public IActionResult ModifyRankCount(int postId, string action,string type)
    {
        if(type == "comment")
        {

        }
        var blogPost = _context.Ranking.FirstOrDefault(b => b.TypeId == postId && b.Type == type && b.User == HttpContext.Session.GetString("Username"));
                    var actualPost = _context.Blogs.FirstOrDefault(b=>b.Id == postId);
            int likeCountForBlog = _context.Ranking.Where(b => b.TypeId == postId).Sum(b => b.Like);
            int dislikeCountForBlog = _context.Ranking.Where(b => b.TypeId == postId).Sum(b => b.Dislike);
            int commentCountForBlog = _context.Comments
                    .Count(comment => comment.BlogId == postId);
        if (blogPost != null)
        {
            if (action == "like")
            {
                if (blogPost.User == HttpContext.Session.GetString("Username") && blogPost.Like != 0)
                {
                    blogPost.Like = 0;
                    actualPost.Popularity = actualPost.Popularity - 2;

                }
                else
                { 
                    blogPost.Like++;
                    actualPost.Popularity = actualPost.Popularity + 2;
                }
            }
            else if (action == "dislike")
            {
                if (blogPost.User == HttpContext.Session.GetString("Username") && blogPost.Dislike == 1)
                {
                    blogPost.Dislike = 0;

                    actualPost.Popularity = actualPost.Popularity + 1;
                }

                else
                {
                    if (blogPost.Like != 0)
                    {
                        blogPost.Like--;
                    }

                    blogPost.Dislike++;
                    actualPost.Popularity--;
                }
            }

            if (type == "blog")
            {
                
                
            }
            int likeCountForBlog1 = _context.Ranking.Where(b => b.TypeId == postId).Sum(b => b.Like);
            int dislikeCountForBlog1 = _context.Ranking.Where(b => b.TypeId == postId).Sum(b => b.Dislike);
            int commentCountForBlog1 = _context.Comments
                    .Count(comment => comment.BlogId == postId);
            blogPost.TypeId = postId;
            //actualPost.Popularity = commentCountForBlog1 + 2 * likeCountForBlog1 - dislikeCountForBlog1;

            _context.SaveChanges();
            var voteCounts = _context.Ranking
    .Where(v => v.TypeId == postId && v.Type == type)
    .GroupBy(v => new { v.TypeId, v.Type })
    .Select(g => new
    {
        Dislike = g.Sum(v => v.Dislike),
        Like = g.Sum(v => v.Like)
    }).FirstOrDefault();
    
            return Json(new { newLike = voteCounts.Like, newDislike = voteCounts.Dislike });

        }
        else
        {
            var newBlogPost = new RankingModel();
            int var = 0;
            int var2 = 0;
            if (action == "like")
            {
                newBlogPost.Like = 1;
                actualPost.Popularity = actualPost.Popularity + 2;
            }
            else if (action == "dislike")
            {
                newBlogPost.Dislike = 1;
                var2 = 1;
                actualPost.Popularity = actualPost.Popularity - 2;
            }
            if (type == "blog")
            {
                newBlogPost.Popularity = commentCountForBlog + 2 * likeCountForBlog - dislikeCountForBlog +  var * 2 - var2;
                newBlogPost.Type = "blog";
            }
            else
            {
                newBlogPost.Popularity = 1;
                newBlogPost.Type = "comment";
            }
            newBlogPost.User = HttpContext.Session.GetString("Username");
                newBlogPost.TypeId = postId;
            actualPost.Popularity = commentCountForBlog + 2 * likeCountForBlog - dislikeCountForBlog;

            _context.Ranking.Add(newBlogPost);

            _context.SaveChanges();
            return Json(new { newLike = newBlogPost.Like,newDislike = newBlogPost.Dislike });
        }
    }
    public IActionResult BlogComments(int blogId)
    {
        // Retrieve all comments for the specified blog from the database
        var comments = _context.Comments
            .Where(c => c.BlogId == blogId)
            .ToList();
        var commentsCounts = _context.Ranking.Where(c => c.Type == "comments" && c.TypeId == blogId);
        

        // Pass the comments to the view or perform further processing
        return Json(new {comments,commentsCounts});
    }

    [HttpPost]
    public async Task<IActionResult> PostComment(int postId, string commentText, string CreatedBy)
    {
        try
        {
            // Validate the incoming data
            if (string.IsNullOrWhiteSpace(commentText) )
            {
                return BadRequest("Comment text and Created by are required.");
            }

            // Create a new comment object

            var comment = new CommentModel
            {
                BlogId = postId,
                Text = commentText,
                CreatedBy = HttpContext.Session.GetString("Username"),
                UserId = int.Parse(HttpContext.Session.GetString("UserId")),
                CreatedDate = DateTime.Now,

            };

            // Add the comment to the database
            var blog = _context.Blogs.Find(postId);
            blog.Popularity++;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new{id=comment.Id,message=comment.Text,createdDate = comment.CreatedDate });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditComment(int commentId, string editedText)
    {
        try
        {
            // Retrieve the comment from the database
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound(); // Comment not found
            }

            // Update the comment text
            comment.Text = editedText;
            comment.LastModifiedDate = DateTime.Now;

            // Update the comment in the database
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            // Return success response
            return Ok(new { message = "Comment updated successfully" });
        }
        catch (Exception ex)
        {
            // Return error response
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }
    public IActionResult DeleteComment (string commentId)
    {
        int parsedId = int.Parse(commentId);
        var commentToDelete = _context.Comments.Find(parsedId);

        // Check if the comment exists
        if (commentToDelete != null)
        {
            // Remove the comment from the context
            var relatedRanks = _context.Ranking
    .Where(v => v.TypeId == parsedId && v.Type == "comment" && v.User == HttpContext.Session.GetString("Username")) // Replace "blog" with the appropriate type for comments
    .ToList();
            int totalPopularity = relatedRanks.Sum(v => v.Popularity);
            var blogToUpdate = _context.Blogs.Find(commentToDelete.BlogId);
            if (blogToUpdate != null)
            {
                blogToUpdate.Popularity--; // Subtract the popularity of the deleted comment
                _context.SaveChanges();
            }
            _context.Comments.Remove(commentToDelete);
            _context.Ranking.RemoveRange(relatedRanks);

            // Save changes to the database

        }
            _context.SaveChanges();
            return Ok( new {success = "Deleted"});
    }

    private async Task<byte[]> TinyImageAsync(IFormFile imageFile)
    {
        using (var inputStream = imageFile.OpenReadStream())
        {
            using (var outputStream = new MemoryStream())
            {
                using (var image = Image.Load(inputStream))
                {
                    var quality = 75; // Initial quality setting

                    // Save the image with the initial quality setting
                    image.Save(outputStream, new JpegEncoder
                    {
                        Quality = quality
                    });

                    // Check the size of the compressed image
                    while (outputStream.Length > 3 * 1024 * 1024) // Check if size exceeds 3 MB
                    {
                        outputStream.SetLength(0); // Reset the output stream
                        quality -= 5; // Reduce quality by 5

                        // Save the image with the adjusted quality
                        image.Save(outputStream, new JpegEncoder
                        {
                            Quality = quality
                        });

                        outputStream.Seek(0, SeekOrigin.Begin); // Reset stream position for next iteration
                    }

                    return outputStream.ToArray();
                }
            }
        }

    }

   

        public IActionResult GetUnreadAlertForUser(Boolean noread)
        {
        // Fetch unread notifications for the specified user, ordered by CreatedAt in descending order


        string userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            // Handle the case where session user ID is not available or not in the correct format
            return BadRequest("Session user ID is missing or invalid.");
        }

        // Fetch unread notifications for the current user
        var unreadAlert = _context.Blogs
            .Join(_context.Alert,
                blog => blog.Id,
                notification => notification.BlogPostId,
                (blog, notification) => new { Blog = blog, Alert = notification })
            .Where(joinResult => joinResult.Blog.UserId == userId )
            .Select(joinResult => new
            {
                Id = joinResult.Alert.Id,
                BlogTitle = joinResult.Blog.Title,
                Message = joinResult.Alert.Message,
                isRead = joinResult.Alert.isRead
            })
            .OrderBy(joinResult => joinResult.isRead)
            .ToList();

        // Get the IDs of the unread notifications
        var notificationIds = unreadAlert.Select(notification => notification.Id).ToList();

        // Update the isRead status of fetched notifications to true
        var notificationsToUpdate = _context.Alert
            .Where(notification => notificationIds.Contains(notification.Id))
            .ToList();

        if(!noread)
            notificationsToUpdate.ForEach(notification => notification.isRead = true);

        // Save changes to the database
        _context.SaveChanges();

        return Json(unreadAlert);

    }
    [HttpPost]
    public IActionResult CreateAlert(int blogPostId, string message)
    {
        try
        {
            // Create a new notification object
            var notification = new AlertModel
            {
                BlogPostId = blogPostId,
                Message = message,
                CreatedAt = DateTime.Now, // You can also use DateTimeOffset.UtcNow for UTC time
                isRead = false // By default, the notification is unread
            };

            // Add the notification to the database context
            _context.Alert.Add(notification);

            // Save changes to the database
            _context.SaveChanges();

            return Ok("Alert Created successfully.");
        }
        catch (Exception ex)
        {
            // Handle exceptions
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}
