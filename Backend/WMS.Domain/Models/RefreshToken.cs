namespace WMS.Domain.Models
{
    public class RefreshToken : BaseModel
    {
        public string TokenHash { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

        public User User { get; set; } = null!;
    }
}
