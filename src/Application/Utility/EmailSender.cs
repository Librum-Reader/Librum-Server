using Application.Interfaces.Managers;
using Application.Interfaces.Utility;
using Domain.Entities;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Application.Utility;

public class EmailSender : IEmailSender
{
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public EmailSender(IAuthenticationManager authenticationManager,
                       IUrlHelper urlHelper,
                       IHttpContextAccessor httpContextAccessor,
                       IConfiguration configuration)
    {
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }
    
    public async Task SendEmailConfirmationEmail(User user, string token)
    {
        var confirmationLink = GetEmailConfirmationLink(user, token);
        
        var message = new MimeMessage();
		// message from Librum if not self-hosted
		if (_configuration["LIBRUM_SELFHOSTED"] != "true"){
			message.From.Add (new MailboxAddress ("Librum", "noreply@librumreader.com"));
		}
        else{
			 var messFrom = _configuration["SMTPMailFrom"];
			 message.From.Add (new MailboxAddress ("Librum", messFrom));
		}
        message.To.Add (new MailboxAddress (user.FirstName, user.Email));
        message.Subject = "Confirm Your Email";
        
        message.Body = new TextPart ("plain") {
            Text = $"Hello { user.FirstName }.\n\nThank you for choosing Librum! " + 
                   "We are happy to tell you, that your account has successfully been created. " +
                   "The final step remaining is to confirm it, and you're all set to go.\n" + 
                   $"To confirm your email, please click the link below:\n{confirmationLink}\n\n" +
                   "If you didn't request this email, just ignore it."
        };

        await SendEmail(message);
    }

    public async Task SendPasswordResetEmail(User user, string token)
    {
		// go to librum site if not selfhosted
		if (_configuration["LIBRUM_SELFHOSTED"] != "true"){
        	var resetLink = $"https://librumreader.com/resetPassword?email={user.Email}&token={token}";
        }
		else {
			var domain =_configuration["CleanUrl"];
			var encodedToken=System.Web.HttpUtility.HtmlEncode(token);
			var resetLink = $"{domain}/user/resetPassword?email={user.Email}&token={encodedToken}";
		}
		
        var message = new MimeMessage();
		// message from librum if not self-hosted
		if (_configuration["LIBRUM_SELFHOSTED"] != "true"){
        	message.From.Add (new MailboxAddress ("Librum", "noreply@librumreader.com"));
		}	
		else{
			var messFrom = _configuration["SMTPMailFrom"];
			message.From.Add (new MailboxAddress ("Librum",messFrom));
		}
        message.To.Add (new MailboxAddress (user.FirstName, user.Email));
        message.Subject = "Reset Your Password";
        
        message.Body = new TextPart ("plain") {
            Text = $"Hello { user.FirstName }.\n\nYou can find the link to reset your password below. " + 
                   "Follow the link and continue the password reset on our website.\n" + 
                   $"{resetLink}\n\n" +
                   "If you didn't request this email, just ignore it."
        };
        
        await SendEmail(message);
    }

    private string GetEmailConfirmationLink(User user, string token)
    {
        var endpointLink = _urlHelper.Action("ConfirmEmail",
                                             "Authentication",
                                             new
                                             {
                                                 email = user.Email,
                                                 token = token
                                             });
        var serverUri = _httpContextAccessor.HttpContext!.Request.Scheme + "://" +
                        _httpContextAccessor.HttpContext!.Request.Host;
        var confirmationLink = serverUri + endpointLink;

        return confirmationLink;
    }

    
    
    private async Task SendEmail(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_configuration["SMTPEndpoint"], 465, true);

        await client.AuthenticateAsync(_configuration["SMTPUsername"],
                                       _configuration["SMTPPassword"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}