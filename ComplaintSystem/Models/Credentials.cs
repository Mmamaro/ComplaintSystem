using System.ComponentModel.DataAnnotations;

namespace ComplaintSystem.Models
{
    public class Credentials
    {
        public string Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class normalLoginCreds
    {
        public string? Token { get; set; }
        public bool? isFirstSignIn { get; set; }
        public bool? isMFA_verified { get; set; }
    }
}
