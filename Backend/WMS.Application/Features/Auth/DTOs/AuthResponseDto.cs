namespace WMS.Application.Features.Auth.DTOs
{
    public class AuthResponseDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
        public bool IsFirstTimeLogin { get; set; }
        public bool RequiresMfa { get; set; }
    }
}

