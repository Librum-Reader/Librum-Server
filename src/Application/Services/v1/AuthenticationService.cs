using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using MailKit.Net.Smtp;
using MimeKit;

namespace Application.Services.v1;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationManager _authenticationManager;


    public AuthenticationService(IMapper mapper,
                                 IAuthenticationManager authenticationManager)
    {
        _mapper = mapper;
        _authenticationManager = authenticationManager;
    }
    
    
    public async Task<string> LoginUserAsync(LoginDto loginDto)
    {
        var email = loginDto.Email;
        var password = loginDto.Password;
        if (await _authenticationManager.UserExistsAsync(email, password))
            return await _authenticationManager.CreateTokenAsync(loginDto);
        
        const string message = "Invalid email or password";
        throw new CommonErrorException(401, message, 1);
    }
    
    public async Task RegisterUserAsync(RegisterDto registerDto)
    {
        if (await _authenticationManager.EmailAlreadyExistsAsync(registerDto.Email))
        {
            const string message = "A user with this email already exists";
            throw new CommonErrorException(400, message, 2);
        }

        var user = _mapper.Map<User>(registerDto);

        var success =
            await _authenticationManager.CreateUserAsync(user, registerDto.Password);
        if (!success)
        {
            const string message = "The provided data was invalid";
            throw new CommonErrorException(400, message, 3);
        }
        
        
        // Send email
        var token = await _authenticationManager.GetEmailConfirmationLinkAsync(user);
        
        var message2 = new MimeMessage();
        message2.From.Add (new MailboxAddress ("Librum", "xxx"));
        message2.To.Add (new MailboxAddress ("xxx", "xxx"));
        message2.Subject = "Test email";
        
        message2.Body = new TextPart ("plain") {
            Text = token
        };
        
        using (var client = new SmtpClient ()) {
            client.Connect ("xx", 465, true);

            // Note: only needed if the SMTP server requires authentication
            client.Authenticate ("xx", "xx");

            client.Send (message2);
            client.Disconnect (true);
        }
    }
    
    public async Task ConfirmEmail(string email, string token)
    {
        var result = await _authenticationManager.ConfirmEmailAsync(email, token);
        if (!result)
            throw new CommonErrorException(400, "Failed confirming email", 0);
    }
}