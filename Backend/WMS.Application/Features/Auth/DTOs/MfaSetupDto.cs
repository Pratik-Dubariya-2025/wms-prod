namespace WMS.Application.Features.Auth.DTOs
{
    public class MfaSetupDto
    {
        public string SharedSecret { get; set; } = null!;
        public string QrCodeUrl { get; set; } = null!;
    }
}
