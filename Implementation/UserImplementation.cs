
using DotNetCourse.Data;
using DotNetCourse.Interfaces;
using DotNetCourse.Models;

namespace DotNetCourse.Implementation
{
    public class UserImplementation : IUserInterface
    {
        private readonly DotNetCourseDbContext _dbContext;

        public UserImplementation(DotNetCourseDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void SignUp(UserModel user)
        {
            // Perform necessary validation
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Optionally, perform additional validation such as checking for unique email, etc.

            // Add user to database
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        
    }
}
