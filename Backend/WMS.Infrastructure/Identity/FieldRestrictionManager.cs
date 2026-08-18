using Microsoft.AspNetCore.Http;
using WMS.Application.Common.Interfaces;

namespace WMS.Infrastructure.Identity
{
    /// <summary>
    /// Implementation of IFieldRestrictionManager that stores restrictions in HttpContext.Items.
    /// </summary>
    public class FieldRestrictionManager(IHttpContextAccessor httpContextAccessor) : IFieldRestrictionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private const string ContextKey = "FieldRestrictions";

        public void SetRestrictions(List<FieldRestrictionResult> restrictions)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items[ContextKey] = restrictions;
            }
        }

        public List<FieldRestrictionResult> GetRestrictions()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Items.TryGetValue(ContextKey, out var val) && val is List<FieldRestrictionResult> list)
            {
                return list;
            }
            return [];
        }

        public bool IsPbacAuthorized
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Items.TryGetValue("PBAC_Authorized", out var val) && val is bool authorized)
                {
                    return authorized;
                }
                return false;
            }
            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["PBAC_Authorized"] = value;
                }
            }
        }

        public string? ModuleCode
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Items.TryGetValue("PBAC_ModuleCode", out var val) && val is string code)
                {
                    return code;
                }
                return null;
            }
            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Items["PBAC_ModuleCode"] = value;
                }
            }
        }
    }
}
