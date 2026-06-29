using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace WebApp.Service.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));

            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = _configuration["Smtp:Port"];
            var smtpUserName = _configuration["Smtp:UserName"];
            var smtpPassword = _configuration["Smtp:Password"];
            var smtpFromEmail = _configuration["Smtp:FromEmail"];
            var smtpFromName = _configuration["Smtp:FromName"];
            var smtpEnableSsl = _configuration["Smtp:EnableSsl"];

            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new InvalidOperationException("SMTP host is not configured.");

            if (!int.TryParse(smtpPort, out var port))
                throw new InvalidOperationException("SMTP port is not configured correctly.");

            if (string.IsNullOrWhiteSpace(smtpFromEmail))
                throw new InvalidOperationException("SMTP from email is not configured.");

            bool.TryParse(smtpEnableSsl, out var enableSsl);

            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(smtpFromName)
                    ? new MailAddress(smtpFromEmail)
                    : new MailAddress(smtpFromEmail, smtpFromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(new MailAddress(toEmail));

            using var smtpClient = new SmtpClient(smtpHost, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(smtpUserName))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUserName, smtpPassword);
            }
            else
            {
                smtpClient.UseDefaultCredentials = true;
            }

            await smtpClient.SendMailAsync(message);
        }
    }
}
