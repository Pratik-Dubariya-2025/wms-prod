namespace WMS.Application.Common.Interfaces
{
    public interface ITotpService
    {
        string GenerateSecret();
        string GetQrCodeUrl(string email, string secret);
        bool VerifyCode(string secret, string code, int timeToleranceStep = 1);
    }
}
