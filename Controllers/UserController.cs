using Microsoft.AspNetCore.Mvc;
using DotNetCourse.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using DotNetCourse.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Options;

public class UserController : Controller
{
    private readonly DotNetCourseDbContext _context;
    private readonly SmtpOptions _smtpOptions;

    public UserController(DotNetCourseDbContext context, IOptions<SmtpOptions> smtpOptions)
    {
        _context = context;
        _smtpOptions = smtpOptions.Value;
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        var model = new UserModel(); // Initialize UserModel with default values
        return View(model);
    }

    [HttpPost]
    public IActionResult SignUp(UserModel model)
    {
        ModelState.Remove("passwordResetToken");
        ModelState.Remove("Blogs");
        ModelState.Remove("Comments");
        if (ModelState.IsValid)
        {
            // Check if the username already exists
            if (_context.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(UserModel.Username), "Username already exists.");
                return View(model);
            }

            // Check if the email already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(UserModel.Email), "Email already exists.");
                return View(model);
            }

            // Add the user to the database
            _context.Users.Add(model);
            _context.SaveChanges();

            // Set a success message to be shown on the login page
            TempData["SignUpSuccessMessage"] = "Successfully signed up!";
            TempData["AdminCreated"] = "Successfully Created User!";
            if (model.Role == UserRole.Admin) return Redirect("/admin");

            // Redirect to the login page
            return RedirectToAction("Login", "User");
        }

        // If the model state is not valid, return the view with validation errors
        return View(model);
    }


    public IActionResult SignUpSuccess()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginModel(); // Initialize LoginViewModel with default values
        return View(model);
    }

    [HttpPost]
    public IActionResult Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Find the user by username
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user != null && user.Password == model.Password)
            {
                if(user.Role.ToString() == "Admin") TempData["AdminLoginMessage"] = "Successfully Logged In";

                else TempData["AdminLoginMessage"] = "Successfully Logged In";

                // User found and password matches, you can set authentication cookies or session here
                // For demonstration purposes, I'm just redirecting to a success page
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
            if (user.Role == UserRole.Admin) return Redirect("/admin");
               // return Redirect("/blog");
            }

            // User not found or password does not match
            TempData["LoginFailureMessage"] = "Invalid username or password";
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
        }
        // If the model state is not valid, return the view with validation errors
        return View(model);
    }

    public IActionResult Logout()
    {
        // Clear session data
        HttpContext.Session.Clear();

        // Redirect to the home page or login page
        return RedirectToAction("Index", "Blog");
    }

    public async Task<IActionResult> EditProfile()
    {
        // Get the user's email from session data
        var userEmail = HttpContext.Session.GetString("Email");

        // Query the database to find the user profile
        var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

        if (userProfile == null)
        {
            // Handle the case where the user profile is not found
            return NotFound();
        }
        

        return View(userProfile);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(UserModel model)
    {
        if (String.IsNullOrEmpty(model.Password)) ModelState.Remove("Password");
        ModelState.Remove("Blogs");
            ModelState.Remove("Comments");
                if (!ModelState.IsValid)
        {
            // If the model state is not valid, return the view with validation errors
            return View(model);
        }

        // Get the user's email from session data
        int Id = int.Parse(HttpContext.Session.GetString("UserId"));

        // Query the database to find the user profile
        var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);

        if (userProfile == null)
        {
            // Handle the case where the user profile is not found
            return NotFound();
        }

        // Update the user profile with the new data
        userProfile.Username = model.Username;
        userProfile.Email = model.Email;
        // You may want to hash the password before storing it in the database
        
        if (!String.IsNullOrEmpty(model.Password)) userProfile.Password = model.Password;

        // Save changes to the database
        await _context.SaveChangesAsync();
        HttpContext.Session.SetString("Username", model.Username);
        HttpContext.Session.SetString("Email", model.Email);
        
       
        TempData["EditProfileSuccessMsg"] = "Profile updated successfully";


        HttpContext.Session.SetString("UserName",model.Username);
        return Redirect("/user/editprofile");
    }

    public IActionResult ForgotPassword()
    {

        return View("PasswordEmailForm");
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        // Validate email (you may also want to check if the email exists in your database)
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("Email", "Email is required");
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if(user == null)
        {
            TempData["ErrorResetPassword"] = "Email not registered";
            return Redirect("/user/forgotpassword");
        }

        string resetToken = Guid.NewGuid().ToString();

        var resetUrl = Url.Action("ResetPassword", "User", new { email = email, token = resetToken }, Request.Scheme);

        // Construct the email message
        var subject = "Password Reset";
        var message = $"Please click the following link to reset your password: {resetUrl}";

        // Send the email
        await SendEmailAsync(email, subject, message);
        TempData["MailSuccess"] = "Password reset link sent successfully";

        // Redirect to a page indicating that an email with password reset instructions has been sent
        return Redirect("/user/forgotPassword");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        // Validate email and token
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            // Handle invalid or missing email/token
            return Redirect("/user/login");
           
        }

       

        var model = new PasswordResetModel { Email = email, token = token };
        return View("PasswordResetForm",model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(PasswordResetModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Retrieve the user by email (you may also want to check if the email exists in your database)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            
            return Redirect("/user/login");
        }

        // Update the user's password in the database
        user.Password = model.Password;
        TempData["myresetSuccess"] = "Your password changed successfully";
       
        // Save changes to the database
        await _context.SaveChangesAsync();

        
        return Redirect("/user/login");
    }




    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpOptions = _smtpOptions; // No need for .Value as you're already accessing the SmtpOptions object

        var smtpClient = new SmtpClient(smtpOptions.ServerAddress, smtpOptions.ServerPort)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpOptions.Username, smtpOptions.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpOptions.Username),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
        
    }
    
    public async Task<IActionResult> DeleteProfile()
    {
        try
        {
            // Get the user ID from the session
            var userId = HttpContext.Session.GetString("UserId");

            // Check if the user ID is valid
            if (string.IsNullOrEmpty(userId))
            {
                // If the user ID is not found in the session, redirect to login page
                return RedirectToAction("Login");
            }

            // Find the user profile based on the user ID
            var userProfile = await _context.Users.FindAsync(int.Parse(userId));

            // Check if the user profile exists
            if (userProfile == null)
            {
                // If the user profile is not found, return a not found response
                return NotFound();
            }

            // Remove the user profile from the database context
            _context.Users.Remove(userProfile);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Clear session data
            HttpContext.Session.Clear();

            // Redirect to the home page or login page
            return RedirectToAction("Index", "Blog");
        }
        catch (Exception ex)
        {
            // Handle exceptions and return an error response
            return StatusCode(500, $"An error occurred while deleting the user profile: {ex.Message}");
        }
    }

}

