using Microsoft.AspNetCore.DataProtection;
using WMS.Application.Common.Interfaces;

namespace WMS.Infrastructure.Identity
{
    public class MfaSecretProtector : IMfaSecretProtector
    {
        private readonly IDataProtector _protector;

        public MfaSecretProtector(IDataProtectionProvider provider)
        {
            // Purpose string scopes this key to exactly this use — even another
            // protector instance elsewhere in the app can't decrypt these values.
            _protector = provider.CreateProtector("WMS.MfaSecret.v1");
        }

        public string Protect(string secret) => _protector.Protect(secret);

        public string Unprotect(string protectedSecret) => _protector.Unprotect(protectedSecret);
    }
}
