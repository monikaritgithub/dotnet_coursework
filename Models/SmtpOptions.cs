namespace DotNetCourse.Models
{
    public class SmtpOptions
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public bool UseSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
