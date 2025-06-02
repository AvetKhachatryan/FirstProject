using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace FirstProject.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public Task SendResetLink(string to, string subject, string body)
        {
            var fromAddress = new MailAddress(_config["Email:Smtp:From"], "MyApp");
            var toAddress = new MailAddress(to);
            string smtpHost = _config["Email:Smtp:Host"];
            int smtpPort = int.Parse(_config["Email:Smtp:Port"]);
            string smtpUser = _config["Email:Smtp:User"];
            string smtpPass = _config["Email:Smtp:Pass"];

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
            return Task.CompletedTask;
        }
    }
}
