namespace WMS.Application.Common.Interfaces
{
    // Encrypts TOTP secrets before they're persisted, so a database read
    // (or a broader compromise of the DB) doesn't hand over live MFA seeds
    // that could be used to generate valid codes for every enrolled user.
    public interface IMfaSecretProtector
    {
        string Protect(string secret);
        string Unprotect(string protectedSecret);
    }
}
