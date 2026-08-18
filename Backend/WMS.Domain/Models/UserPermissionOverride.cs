using System;

namespace WMS.Domain.Models
{
    public class UserPermissionOverride : BaseModel
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public bool IsGranted { get; set; }
        public string? Reason { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
