using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;

namespace MagicApps.Infrastructure.Services
{
    public class EmailService : IIdentityMessageService
    {
        // Credentials:
        private string credentialUserName = ConfigurationManager.AppSettings["Settings.Mail.UserName"];
        private string sentFrom = ConfigurationManager.AppSettings["Settings.Mail.From"];
        private string pwd = ConfigurationManager.AppSettings["Settings.Mail.Password"];
        private string host = ConfigurationManager.AppSettings["Settings.Mail.Server"];
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["Settings.Mail.Port"]);
        private bool useSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["Settings.Mail.UseSSL"]);
        private bool writeAsFile = Convert.ToBoolean(ConfigurationManager.AppSettings["Settings.Mail.WriteAsFile"]);
        private string fileLocation = ConfigurationManager.AppSettings["Settings.Mail.FileLocation"];

        private SmtpClient client;
        private System.Net.NetworkCredential credentials;

        public List<string> Errors { get; private set; }

        public EmailService()
        {
            // Configure the client:
            client = new SmtpClient(host);

            client.Port = port;
            client.UseDefaultCredentials = false;

            // Create the credentials:
            credentials = new System.Net.NetworkCredential(credentialUserName, pwd);

            if (writeAsFile) {
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = fileLocation;
            }
            else {
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
            }

            client.EnableSsl = useSSL;
            client.Credentials = credentials;

            Errors = new List<string>();
        }

        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.

            // Create the message:
            var mail =
                new MailMessage(sentFrom, message.Destination);

            mail.Subject = message.Subject;
            mail.Body = GenerateHtmlBody(message.Subject, message.Body);
            mail.IsBodyHtml = true;

            // Send:
            try {
                return client.SendMailAsync(mail);
            }
            catch {
                return Task.FromResult(0);
            }
        }

        public string SendMail(string subject, string body, string recipient, List<string> attachments = null)
        {
            MailAddress mailAddress = ParseRecipient(recipient);            

            return SendMailWork(subject, body, mailAddress, ParseAttachments(attachments));
        }

        public IEnumerable<string> SendMail(string subject, string body, List<string> recipients, List<string> attachments = null)
        {
            // Statuses
            var statuses = new List<string>();

            // if no recipients are set, send to the Admin
            MailAddressCollection mailAddresses = new MailAddressCollection();

            foreach (string recipient in recipients) {
                mailAddresses.Add(ParseRecipient(recipient));
            }

            List<Attachment> _attachments = ParseAttachments(attachments);

            foreach (MailAddress recipient in mailAddresses) {
                statuses.Add(SendMailWork(subject, body, recipient, _attachments));
            }

            return statuses;
        }

        public int ErrorCount()
        {
            return Errors.Count;
        }

        public string ParseErrorsAsUL()
        {
            if (ErrorCount() == 0) {
                return string.Empty;
            }

            string r = "<ul>";

            foreach (string err in Errors) {
                r += "<li>" + err + "</li>";
            }

            r += "</ul>";

            return r;
        }

        private string SendMailWork(string subject, string body, MailAddress recipient, List<Attachment> attachments)
        {
            string status;
            string mailBody = body;

            MailMessage msg = new MailMessage() {
                From = new MailAddress(sentFrom),
                Subject = subject,
                IsBodyHtml = true
            };

            // Add the recipient
            msg.To.Add(recipient);

            // Parse Body
            if (!String.IsNullOrEmpty(recipient.DisplayName)) {
                mailBody = body.Replace("[FullName]", recipient.DisplayName);
            }

            mailBody = GenerateHtmlBody(subject, mailBody);

            if (writeAsFile) {
                msg.BodyEncoding = System.Text.Encoding.ASCII;
            }

            msg.Body = mailBody;

            // Add any attachments
            if (attachments != null) {
                foreach (var attachment in attachments) {
                    msg.Attachments.Add(attachment);
                }
            }

            try {
                client.Send(msg);
                status = String.Format("Success: message delivered to {0}", recipient.Address);
            }
            catch (Exception e) {
                status = String.Format("Error: {0}", e.Message);
                Errors.Add(string.Format("Failed to send message to {0}. Error: {1}", recipient.Address, e.Message));
            }

            return status;
        }

        private List<Attachment> ParseAttachments(List<string> attachments)
        {
            if (attachments == null) {
                return null;
            }

            // Convert attachments into a list of MailAttachments
            List<Attachment> _attachments = null;

            if (attachments != null) {
                _attachments = new List<Attachment>();

                foreach (var item in attachments) {
                    _attachments.Add(new Attachment(item));
                }
            }

            return _attachments;
        }

        private MailAddress ParseRecipient(string recipient)
        {
            if (recipient.Contains(":")) // is the entry a email:name pairing
            {
                string eml, name;
                int idx = recipient.IndexOf(":");

                eml = recipient.Substring(0, idx);
                name = recipient.Substring((idx + 1), ((recipient.Length - eml.Length) - 1));

                return new MailAddress(eml, name);
            }
            else {
                return new MailAddress(recipient);
            }
        }

        private string GenerateHtmlBody(string subject, string body)
        {
            System.Text.StringBuilder hBody = new System.Text.StringBuilder();

            hBody.AppendLine("<!DOCTYPE html>")
                .AppendLine("<html>")
                .AppendLine("<head>")
                .AppendLine(@"<meta charset=""utf-8"" />")
                .AppendFormat("<title>{0}</title>", subject)
                .AppendLine(@"<style type=""text/css"">")
                .AppendLine("body { font-family: Helvetica, Arial, sans-serif; font-size: 14px; color: #333; }")
                .AppendLine("h1, h2 { font-size: 2em; font-weight: normal; color: #b3303d; margin-bottom: .5em; }")
                .AppendLine("h3 { font-size: 1.4em; font-weight: normal; }")
                .AppendLine("hr { margin:1em 0; }")
                .AppendLine("h4, h5, h6, td, th { margin:0; padding:0; }")
                .AppendLine(@"table { border: 1px solid silver; border-spacing: 0px; border-collapse: collapse; margin-bottom: 20px; }")
                .AppendLine(@"table td { padding: 5px; border: 1px solid silver; }")
                .AppendLine(@"table td.label { vertical-align: top; }")
                .AppendLine(@"table td.form-label { width: 30%; }")
                .AppendLine(@"table td .inline-label { display: inline-block; width: 200px; }")
                .AppendLine(@"table td.nowrap { white-space: nowrap; }")
                .AppendLine("#content { max-width:600px; margin:0px auto; }")
                .AppendLine(".panel { height: auto; overflow: hidden; margin: 20px 0px; border: solid 1px #ddd; padding: 15px; }")
                .AppendLine(".footer-text { font-size: .9em; padding-top: 15px; }")
                .AppendLine("</style>")
                .AppendLine("</head>")
                .AppendLine("<body>")
                .AppendLine(@"<div id=""content"">")
                .AppendLine(body)
                .AppendLine("</div>")
                .AppendLine("</body>")
                .AppendLine("</html>");

            return hBody.ToString();
        }
    }
}