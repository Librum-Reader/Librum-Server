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
    private readonly IAuthenticationManager _authenticationManager;
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public EmailSender(IAuthenticationManager authenticationManager,
                       IUrlHelper urlHelper,
                       IHttpContextAccessor httpContextAccessor,
                       IConfiguration configuration)
    {
        _authenticationManager = authenticationManager;
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }
    
    public async Task SendAccountConfirmationEmail(User user)
    {
        var confirmationLink = await GetAccountConfirmationLink(user);
        
        var message = new MimeMessage();
        message.From.Add (new MailboxAddress ("Librum", "noreply@librumreader.com"));
        message.To.Add (new MailboxAddress (user.FirstName, "prtnprvtmail@protonmail.com"));
        message.Subject = "Confirm Your Librum Account";
        
        message.Body = new TextPart ("plain") {
            Text = $"Hello { user.FirstName }.\n\nThank you for choosing Librum! " + 
                   "We are happy to tell you, that your account has successfully been created. " +
                   "The final step remaining is to confirm it, and you're all set to go.\n" + 
                   $"To confirm your account, please click the link below:\n{confirmationLink}\n\n" +
                   "If you didn't request this email, just ignore it."
        };

        await SendEmail(message);
    }

    private async Task<string> GetAccountConfirmationLink(User user)
    {
        var token = await _authenticationManager.GetEmailConfirmationLinkAsync(user);
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
        using (var client = new SmtpClient()) {
            await client.ConnectAsync(_configuration["SMTPEndpoint"], 465, true);

            await client.AuthenticateAsync(_configuration["SMTPUsername"],
                                           _configuration["SMTPPassword"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}