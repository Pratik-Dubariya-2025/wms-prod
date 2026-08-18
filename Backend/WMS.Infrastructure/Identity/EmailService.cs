using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WMS.Application.Common.Interfaces;

namespace WMS.Infrastructure.Identity
{
    public class EmailService(ILogger<EmailService> logger, IConfiguration configuration) : IEmailService
    {
        private readonly ILogger<EmailService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("Attempting to send email to {To} with subject '{Subject}'", to, subject);

            string host = (_configuration["SmtpSettings:Host"] ?? string.Empty).Trim();
            string username = (_configuration["SmtpSettings:Username"] ?? string.Empty).Trim();
            string password = (_configuration["SmtpSettings:Password"] ?? string.Empty).Trim();
            string senderEmail = (_configuration["SmtpSettings:SenderEmail"] ?? string.Empty).Trim();
            string senderName = _configuration["SmtpSettings:SenderName"] ?? "WMS Admin";

            int port = 587;
            int.TryParse(_configuration["SmtpSettings:Port"], out port);
            
            bool enableSsl = true;
            bool.TryParse(_configuration["SmtpSettings:EnableSsl"], out enableSsl);

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || username.Contains("your-email"))
            {
                _logger.LogWarning("SMTP Settings are not configured. Real email was not sent.");
                return;
            }

            try
            {
                using (MailMessage mailMessage = new())
                {
                    mailMessage.From = new MailAddress(senderEmail, senderName);
                    mailMessage.To.Add(new MailAddress(to));
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    using (SmtpClient smtpClient = new(host, port))
                    {
                        smtpClient.Credentials = new NetworkCredential(username, password);
                        smtpClient.EnableSsl = enableSsl;
                        
                        await smtpClient.SendMailAsync(mailMessage);
                        _logger.LogInformation("Email successfully sent via SMTP to {To}", to);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} via SMTP", to);
                throw;
            }
        }
    }
}
