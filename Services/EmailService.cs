using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Fetch email configuration
        var fromEmail = _config["EmailSettings:FromEmail"];
        var fromName = _config["EmailSettings:FromName"];
        var smtpHost = _config["EmailSettings:SmtpHost"];
        var smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"], out int port) ? port : 587;
        var smtpUser = _config["EmailSettings:SmtpUser"];
        var smtpPass = _config["EmailSettings:SmtpPass"];

        // Construct the email message
        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        // Configure SMTP client and send email
        using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
        {
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPass);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtpClient.SendMailAsync(message);
        }
    }

    public string GetOtpEmailTemplate(string name, string otp)
    {
        return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                padding: 30px;
            }}
            .container {{
                max-width: 600px;
                margin: auto;
                background-color: #fff;
                border-radius: 8px;
                padding: 30px;
                box-shadow: 0 0 10px rgba(0,0,0,0.1);
            }}
            h2 {{
                color: #2c3e50;
            }}
            .otp-box {{
                font-size: 22px;
                color: #ffffff;
                background-color: #2c3e50;
                padding: 10px 20px;
                display: inline-block;
                border-radius: 4px;
                margin: 20px 0;
                letter-spacing: 3px;
            }}
            p {{
                color: #555;
            }}
            .footer {{
                font-size: 12px;
                color: #999;
                margin-top: 30px;
                text-align: center;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>Hello {name},</h2>
            <p>You recently requested to reset your password. Please use the following OTP to proceed:</p>
            <div class='otp-box'>{otp}</div>
            <p>This OTP is valid for 10 minutes. Please do not share this code with anyone.</p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
            <div class='footer'>
                &copy; {DateTime.Now.Year} HMC Snacks. All rights reserved.
            </div>
        </div>
    </body>
    </html>";
    }

    public string GetLoginOtpEmailTemplate(string name, string otp)
    {
        return $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            padding: 30px;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background-color: #fff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        h2 {{
            color: #2c3e50;
        }}
        .otp-box {{
            font-size: 22px;
            color: #ffffff;
            background-color: #2c3e50;
            padding: 10px 20px;
            display: inline-block;
            border-radius: 4px;
            margin: 20px 0;
            letter-spacing: 3px;
        }}
        p {{
            color: #555;
        }}
        .footer {{
            font-size: 12px;
            color: #999;
            margin-top: 30px;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Hello {name},</h2>
        <p>Welcome back! To complete your login to <strong>HMC Snacks</strong>, please use the OTP below:</p>
        <div class='otp-box'>{otp}</div>
        <p>This OTP is valid for 10 minutes. Please do not share it with anyone.</p>
        <p>If you didn’t attempt to log in, you can safely ignore this email.</p>
        <div class='footer'>
            &copy; {DateTime.Now.Year} HMC Snacks. All rights reserved.
        </div>
    </div>
</body>
</html>";
    }



    public string SendWelcomeEmailAsync(string name)
    {
        return $@"
        <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='padding: 20px; background-color: #f4f4f4; border-radius: 10px;'>
                    <h2>Welcome, {name}! 👋</h2>
                    <p>Thank you for registering with <strong>HMC Snacks</strong>.</p>
                    <p>We’re excited to have you on board. If you have any questions, just reply to this email—we’re always happy to help!</p>
                    <br/>
                    <p>Warm regards,<br/>Team HMC Snacks</p>
                </div>
            </body>
        </html>
    ";
    }

}
