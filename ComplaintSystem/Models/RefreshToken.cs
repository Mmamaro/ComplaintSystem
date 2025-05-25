

namespace ComplaintSystem.Models
{

    public class RefreshToken
    {
        public Guid? Id { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpiresOn { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Email { get; set; }
        public string RefreshToken { get; set; }
    }
}
