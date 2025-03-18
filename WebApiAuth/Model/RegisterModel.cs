namespace WebApiAuth.Model
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
