using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;   // ✅ السطر المهم
using System.Text;


namespace NetReach.Api.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.hostinger.com";
        private readonly int _smtpPort = 465;
        private readonly string _senderEmail = "info@netreach.site";
        private readonly string _senderPassword = "Losha55**";

        public async Task SendProductEmailAsync(string recipientEmail, string productName, List<string> items)
        {
            // 🛡️ PROTECTION: Check if items is empty
            if (items == null || items.Count == 0)
            {
                Console.WriteLine("[Email] ❌ ERROR: No items to send!");
                throw new Exception("No items available to send");
            }

            Console.WriteLine($"[Email] Preparing email for {recipientEmail}");
            Console.WriteLine($"[Email] Product: {productName}");
            Console.WriteLine($"[Email] Items count: {items.Count}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NetReach Store", _senderEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.ReplyTo.Add(new MailboxAddress("NetReach Support", _senderEmail));
            
            string orderId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            message.Subject = $"✅ Your {productName} - Order #{orderId}";
            
            // Set important headers for spam prevention
            message.MessageId = MimeUtils.GenerateMessageId();
            message.Date = DateTimeOffset.Now;
            message.Headers.Add("X-Priority", "1");
            message.Headers.Add("Importance", "high");

            // Build HTML email
            string htmlContent = BuildHtmlTemplate(productName, items, orderId, recipientEmail);

            // Create multipart message
            var multipart = new Multipart("mixed");

            // Add HTML body
            var htmlPart = new TextPart("html") { Text = htmlContent };
            multipart.Add(htmlPart);

            // Add text file attachment
            string fileData = BuildTextFile(productName, items, orderId);
            var attachmentBytes = Encoding.UTF8.GetBytes(fileData);

            var attachment = new MimePart("text", "plain")
            {
                Content = new MimeContent(new MemoryStream(attachmentBytes)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = $"NetReach_{productName.Replace(" ", "_")}_{orderId}.txt"
            };
            
            multipart.Add(attachment);
            message.Body = multipart;

            // Send email
            using var client = new SmtpClient();
            try
            {
                Console.WriteLine($"[Email] Connecting to {_smtpServer}:{_smtpPort}");
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.SslOnConnect);
                
                Console.WriteLine($"[Email] Authenticating...");
                await client.AuthenticateAsync(_senderEmail, _senderPassword);
                
                Console.WriteLine($"[Email] Sending email...");
                await client.SendAsync(message);
                
                Console.WriteLine($"[Email] ✅ Email sent successfully to {recipientEmail}");
                
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email] ❌ Error: {ex.Message}");
                Console.WriteLine($"[Email] Stack: {ex.StackTrace}");
                throw;
            }
        }

        private string BuildHtmlTemplate(string productName, List<string> items, string orderId, string email)
        {
            // Format items for HTML display
            var itemsHtml = new StringBuilder();
            for (int i = 0; i < items.Count; i++)
            {
                itemsHtml.Append($@"
                <div style='background: #f8f9fa; padding: 15px; margin: 10px 0; border-left: 4px solid #667eea; border-radius: 6px;'>
                    <div style='color: #666; font-size: 12px; margin-bottom: 5px;'>Item #{i + 1}</div>
                    <div style='font-family: Courier New, monospace; font-size: 13px; color: #333; word-break: break-all; line-height: 1.6;'>
                        {System.Web.HttpUtility.HtmlEncode(items[i])}
                    </div>
                </div>");
            }

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Arial, sans-serif; background-color: #f4f6f9;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f6f9; padding: 40px 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='max-width: 600px; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 8px 24px rgba(0,0,0,0.12);'>
                    
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 50px 40px; text-align: center;'>
                            <div style='font-size: 48px; margin-bottom: 10px;'>🎉</div>
                            <h1 style='margin: 0; color: #ffffff; font-size: 32px; font-weight: 700; letter-spacing: -0.5px;'>
                                Order Delivered!
                            </h1>
                            <p style='margin: 12px 0 0 0; color: #ffffff; font-size: 16px; opacity: 0.95;'>
                                Your purchase is ready to use
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style='padding: 45px 40px;'>
                            
                            <p style='margin: 0 0 30px 0; color: #333; font-size: 16px; line-height: 1.7;'>
                                Hi there! 👋<br><br>
                                Thank you for choosing <strong>NetReach</strong>. Your order has been successfully processed and delivered.
                            </p>
                            
                            <!-- Order Info Box -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='background: linear-gradient(to right, #f8f9fa, #e9ecef); border-radius: 12px; padding: 25px; margin: 30px 0;'>
                                <tr>
                                    <td>
                                        <h3 style='margin: 0 0 18px 0; color: #333; font-size: 19px; font-weight: 700;'>
                                            📦 Order Summary
                                        </h3>
                                        <table width='100%' cellpadding='8' cellspacing='0'>
                                            <tr>
                                                <td style='color: #666; font-size: 15px; padding: 8px 0;'>Order ID:</td>
                                                <td style='color: #333; font-size: 15px; font-weight: 600; text-align: right; font-family: Courier New, monospace;'>#{orderId}</td>
                                            </tr>
                                            <tr>
                                                <td style='color: #666; font-size: 15px; padding: 8px 0;'>Product:</td>
                                                <td style='color: #667eea; font-size: 15px; font-weight: 700; text-align: right;'>{productName}</td>
                                            </tr>
                                            <tr>
                                                <td style='color: #666; font-size: 15px; padding: 8px 0;'>Quantity:</td>
                                                <td style='color: #333; font-size: 15px; font-weight: 600; text-align: right;'>{items.Count} item{(items.Count > 1 ? "s" : "")}</td>
                                            </tr>
                                            <tr>
                                                <td style='color: #666; font-size: 15px; padding: 8px 0;'>Date:</td>
                                                <td style='color: #333; font-size: 15px; font-weight: 600; text-align: right;'>{DateTime.UtcNow:MMM dd, yyyy HH:mm} UTC</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            
                            <!-- Items Section -->
                            <div style='margin: 35px 0;'>
                                <h3 style='margin: 0 0 20px 0; color: #333; font-size: 19px; font-weight: 700;'>
                                    🔐 Your Account Details
                                </h3>
                                <p style='margin: 0 0 18px 0; color: #666; font-size: 14px; line-height: 1.6;'>
                                    Below are your account credentials. We recommend saving this information securely. A backup text file is also attached to this email.
                                </p>
                                {itemsHtml}
                            </div>
                        
                            <!-- Support Section -->
                            <div style='margin: 40px 0 0 0; padding: 30px 0; border-top: 2px solid #e9ecef;'>
    <h4 style='margin: 0 0 15px 0; color: #333; font-size: 17px; font-weight: 600;'>
        💬 Need Help?
    </h4>
    <p style='margin: 0 0 12px 0; color: #666; font-size: 15px; line-height: 1.7;'>
        Our support team is ready to assist you 24/7:
    </p>

    <p style='margin: 0 0 8px 0; font-size: 15px;'>
        📧 <a href='mailto:{_senderEmail}' style='color: #667eea; text-decoration: none; font-weight: 600;'>{_senderEmail}</a>
    </p>

    <p style='margin: 0; font-size: 15px;'>
        📲 <span style='color: #333; font-weight: 600;'>Telegram:</span>
        <a href='https://t.me/netreach_team' 
           style='color: #667eea; text-decoration: none; font-weight: 600;'>
            @netreach_team
        </a>
    </p>
</div>

                            
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style='background: #f8f9fa; padding: 35px 40px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0 0 10px 0; color: #999; font-size: 14px; line-height: 1.6;'>
                                © {DateTime.UtcNow.Year} <strong>NetReach</strong>. All rights reserved.
                            </p>
                            <p style='margin: 0 0 15px 0; color: #999; font-size: 13px;'>
                                This email was sent to <span style='color: #667eea; font-weight: 600;'>{email}</span>
                            </p>
                            <p style='margin: 0; color: #bbb; font-size: 12px;'>
                                This is an automated message, please do not reply directly.
                            </p>
                        </td>
                    </tr>
                    
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string BuildTextFile(string productName, List<string> items, string orderId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine("           NETREACH ORDER CONFIRMATION          ");
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Order ID: #{orderId}");
            sb.AppendLine($"Product: {productName}");
            sb.AppendLine($"Quantity: {items.Count}");
            sb.AppendLine($"Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine("              YOUR ACCOUNT DETAILS              ");
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine();
            
            for (int i = 0; i < items.Count; i++)
            {
                sb.AppendLine($"Item #{i + 1}:");
                sb.AppendLine(items[i]);
                sb.AppendLine();
                sb.AppendLine("-------------------------------------------");
                sb.AppendLine();
            }
            
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine("                   SUPPORT                     ");
            sb.AppendLine("═══════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"Email: {_senderEmail}");
            sb.AppendLine("Telegram: @netreach_team");
            sb.AppendLine("We're here to help 24/7!");
            sb.AppendLine();
            sb.AppendLine($"© {DateTime.UtcNow.Year} NetReach. All rights reserved.");
            
            return sb.ToString();
        }
    }
}