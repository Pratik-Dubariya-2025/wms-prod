namespace WMS.Domain.Models
{
    public class ForgotPassword : BaseModel
    {
        public string TokenHash { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
