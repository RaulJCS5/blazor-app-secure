namespace BlazorAppIdentity.Model
{
    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public bool isEmailConfirmed { get; set; }
        public Dictionary<string, string> Claims { get; set; } = [];
    }
}
