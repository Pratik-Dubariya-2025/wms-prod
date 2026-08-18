using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Identity
{
    public class TokenService(IConfiguration configuration, IUnitOfWork unitOfWork) : ITokenService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        /// <summary>
        /// Generates a JWT access token containing all user identity claims
        /// and role claims for authorization.
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            string secret = jwtSettings["Secret"]!;
            string issuer = jwtSettings["Issuer"]!;
            string audience = jwtSettings["Audience"]!;
            int expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(secret));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("UserId", user.Id.ToString()),
                new("Username", user.Username),
                new("EmployeeCode", user.EmployeeCode),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName),
                new("IsMfaEnabled", user.IsMfaEnabled.ToString().ToLower())
            ];

            if (user.Department != null)
            {
                claims.Add(new("DepartmentId", user.Department.Id.ToString()));
                claims.Add(new("DepartmentName", user.Department.Name));
                claims.Add(new("DepartmentCode", user.Department.Code));
            }

            if (user.Designation != null)
            {
                claims.Add(new("DesignationId", user.Designation.Id.ToString()));
                claims.Add(new("DesignationName", user.Designation.Name));
                claims.Add(new("DesignationCode", user.Designation.Code));
            }

            // Role claims drive authorization (RBAC). If the caller didn't eager-load
            // roles, fetch them here so every issued token carries them.
            List<string> roleClaims = [];
            if (user.UserRoles != null && user.UserRoles.Any())
            {
                foreach (var ur in user.UserRoles.Where(ur => ur.Role != null && !ur.Role.IsDeleted))
                {
                    roleClaims.Add(ur.Role.Name);
                    roleClaims.Add(ur.Role.Code);
                }
            }

            if (!roleClaims.Any())
            {
                var dbRoles = _unitOfWork.UserRole.GetAllAsync(
                    select: ur => new { ur.Role.Name, ur.Role.Code },
                    where: ur => ur.UserId == user.Id && !ur.Role.IsDeleted)
                    .GetAwaiter().GetResult();
                
                foreach (var r in dbRoles)
                {
                    roleClaims.Add(r.Name);
                    roleClaims.Add(r.Code);
                }
            }

            foreach (var roleClaim in roleClaims.Distinct())
            {
                claims.Add(new Claim(ClaimTypes.Role, roleClaim));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token (Base64 string).
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Validates an expired JWT token and returns its ClaimsPrincipal.
        /// Used during the refresh token flow to extract user identity
        /// from an expired access token without requiring it to still be valid.
        /// </summary>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            string secret = jwtSettings["Secret"]!;

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Allow expired tokens
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };

            JwtSecurityTokenHandler tokenHandler = new();

            try
            {
                ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
