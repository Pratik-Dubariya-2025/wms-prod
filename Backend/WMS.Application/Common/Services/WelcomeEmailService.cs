using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WMS.Application.Common.Interfaces;

namespace WMS.Application.Common.Services
{
    /// <summary>
    /// Loads the embedded welcome template and dispatches the invitation email.
    /// A failure here is logged but swallowed so it never rolls back user
    /// creation — the caller has already committed the user.
    /// </summary>
    public class WelcomeEmailService(
        IEmailService emailService,
        ILogger<WelcomeEmailService> logger) : IWelcomeEmailService
    {
        private const string TemplateResourceName = "WMS.Application.Common.Templates.WelcomeUserTemplate.html";
        private const string LoginLink = "http://localhost:5173/login";
        private const string Subject = "Welcome to Workspace Management System";

        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<WelcomeEmailService> _logger = logger;

        public async Task SendWelcomeEmailAsync(
            string toEmail,
            string firstName,
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string template = await LoadTemplateAsync(cancellationToken);
                string body = !string.IsNullOrEmpty(template)
                    ? string.Format(template, firstName, username, password, LoginLink)
                    : BuildFallbackBody(firstName, username, password);

                await _emailService.SendEmailAsync(toEmail, Subject, body);
            }
            catch (Exception ex)
            {
                // Email delivery must never fail the (already committed) invite.
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
            }
        }

        private static async Task<string> LoadTemplateAsync(CancellationToken cancellationToken)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            await using Stream? stream = assembly.GetManifestResourceStream(TemplateResourceName);
            if (stream is null)
            {
                return string.Empty;
            }

            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        private static string BuildFallbackBody(string firstName, string username, string password) =>
            $"""
            Hi {firstName},

            Your account has been created in the Workspace Management System.

            Username: {username}
            Password: {password}

            Please log in and change your password immediately: {LoginLink}

            Regards,
            WMS Admin
            """;
    }
}
