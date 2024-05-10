//using DotNetCourse.Data;
//using DotNetCourse.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//public class AlertHub : Hub
//{
//    private DotNetCourseDbContext _context;
//    private IHttpContextAccessor _contextAccessor;
//    private static readonly Dictionary<string, string> _userConnectionMap = new Dictionary<string, string>();

//    public AlertHub(DotNetCourseDbContext context, IHttpContextAccessor httpContextAccessor)
//    {
//        _context = context;
//        _contextAccessor = httpContextAccessor;
//    }

//    public override async Task OnConnectedAsync()
//    {
//        // Get the user ID from the current context (e.g., from claims)
//        var userId = _contextAccessor.HttpContext.Session.GetString("UserId");
//        Context.Items["UserId"] = userId; // Store user ID in the connection context

//        // Add the connection to the user connection map
//        _userConnectionMap[userId] = Context.ConnectionId;

//        await base.OnConnectedAsync();
//    }


//    public override async Task OnDisconnectedAsync(Exception exception)
//    {
//        // Remove the disconnected connection from the user connection map
//        var userId = _userConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
//        if (!string.IsNullOrEmpty(userId))
//        {
//            _userConnectionMap.Remove(userId);
//        }

//        await base.OnDisconnectedAsync(exception);
//    }

//    public async Task SendMessage(string userId, string message)
//    {
//        // Create a new notification
//        var userIds = _userConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

//        var notification = new AlertModel
//        {
//            BlogPostId = int.Parse(userId), // Assuming userId represents the associated blog post id
//            Message = message,
//            CreatedAt = DateTime.Now,
//            isRead = false // Set the notification as unread initially
//        };

//        // Add the notification to the database
//        _context.Alert.Add(notification);
//        await _context.SaveChangesAsync();
//        if (userIds != null && userIds.Equals(userId.ToString()))
//        {
//            await SendAlertToAuthor(int.Parse(userId), message);
//        }
//        // Send the notification to the author
//        //await SendAlertToAuthor(userId, message);
//    }

//    private async Task SendAlertToAuthor(int blogPostId, string message)
//    {
//        // Retrieve the author's user id based on the blog post id
//        var authorUserId = await _context.Blogs
//            .Where(blog => blog.Id == blogPostId)
//            .Select(blog => blog.UserId)
//            .FirstOrDefaultAsync();

//        // If the author user id is found, send the notification to the author
//        if (authorUserId != default(int))
//        {

//            var userIds = _userConnectionMap.FirstOrDefault(x => x.Value == Context.ConnectionId).Value;

//            // Check if there are any unread notifications for the author
//            var unreadAlert = await _context.Alert
//                .Where(notification => notification.BlogPostId == blogPostId && !notification.isRead)
//                .OrderByDescending(notification => notification.CreatedAt)
//                .Take(10)
//                .ToListAsync();

//            // If there are unread notifications, send them to the author
//            if (unreadAlert.Any())
//            {
//                // Send notifications to a specific user
//                await Clients.User(userIds).SendAsync("ReceiveNotificatcatonions", unreadAlert);

//            }
//            else
//            {
//                // If all notifications are read, send the latest 10 read notifications to the author
//                var readAlert = await _context.Alert
//                    .Where(notification => notification.BlogPostId == blogPostId && notification.isRead)
//                    .OrderByDescending(notification => notification.CreatedAt)
//                    .Take(10)
//                    .ToListAsync();

//                await Clients.All   .SendAsync("ReceiveAlert", readAlert);
//            }
//        }
//    }

//}

