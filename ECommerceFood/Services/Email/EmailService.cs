using System.Net.Mail;

namespace ECommerceFood.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SMTPServer"] ?? throw new ArgumentNullException(nameof(configuration), "SMTPServer is not configured.");
            _port = int.Parse(configuration["EmailSettings:SMTPPort"] ?? throw new ArgumentNullException(nameof(configuration), "SMTPPort is not configured."));
            _username = configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException(nameof(configuration), "SenderEmail is not configured.");
            _password = configuration["EmailSettings:SenderPassword"] ?? throw new ArgumentNullException(nameof(configuration), "SenderPassword is not configured.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_username),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(_smtpServer, _port))
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(_username, _password);
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
