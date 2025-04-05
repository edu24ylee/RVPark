using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("tawnymcaleese@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart("html") { Text = htmlMessage };

            using var emailClient = new SmtpClient();

            // ✅ Disable SSL validation for dev — WARNING: do not use in production!
            emailClient.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;

            await emailClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await emailClient.AuthenticateAsync("tawnymcaleese@gmail.com", "knqpltsevsaqwvok");
            await emailClient.SendAsync(emailToSend);
            await emailClient.DisconnectAsync(true);
        }
    }
}
