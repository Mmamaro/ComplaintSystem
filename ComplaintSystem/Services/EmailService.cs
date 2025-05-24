using static QRCoder.PayloadGenerator;
using System.Net.Mail;
using System.Net;
using Serilog;

namespace ComplaintSystem.Helpers
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }


        #region [ Send Template Email ]

        public void SendTemplateEmail(string recipent, string subject, string htmlbody)
        {
            try
            {
                string smtpServer = _config["EmailSettings:SMTPServer"]!;
                int smtpPort = int.Parse(_config["EmailSettings:SMTPServerPort"]!);
                bool enableSSL = bool.Parse(_config["EmailSettings:EnableSsl"]!);
                string fromEmail = _config["EmailSettings:EmailSender"]!;
                string Password = _config["EmailSettings:SMTPPassword"]!;



                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    EnableSsl = enableSSL,
                    Credentials = new NetworkCredential(fromEmail,Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = htmlbody,
                    IsBodyHtml = true
                };

                 mailMessage.To.Add(recipent); 


                smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in the Email Helper while trying to send an email with a template.");
                throw;
            }
        }

        #endregion
    }
}
