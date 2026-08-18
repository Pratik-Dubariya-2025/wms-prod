namespace WMS.Application.Features.Users.DTOs
{
    public class UserPermissionOverrideDto
    {
        public string PermissionCode { get; set; } = null!;
        public bool IsGranted { get; set; }
        public string? Reason { get; set; }
        public System.DateTime? ExpiresAt { get; set; }
    }
}
