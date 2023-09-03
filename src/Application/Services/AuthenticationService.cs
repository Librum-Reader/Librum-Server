using System.Web;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Managers;
using Application.Interfaces.Services;
using Application.Interfaces.Utility;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationManager _authenticationManager;
    private readonly IEmailSender _emailSender;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;


    public AuthenticationService(IMapper mapper,
                                 IAuthenticationManager authenticationManager,
                                 IEmailSender emailSender,
                                 IHttpClientFactory httpClientFactory,
                                 IConfiguration configuration)
    {
        _mapper = mapper;
        _authenticationManager = authenticationManager;
        _emailSender = emailSender;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    
    public async Task<string> LoginUserAsync(LoginDto loginDto)
    {
        var email = loginDto.Email;
        var password = loginDto.Password;

        if (!await _authenticationManager.UserExistsAsync(email, password))
        {
            const string message = "Invalid email or password";
            throw new CommonErrorException(401, message, 1);
        }

        if (!await _authenticationManager.IsEmailConfirmed(email))
        {
            const string message = "Email is not confirmed";
            throw new CommonErrorException(401, message, 18);
        }

        return await _authenticationManager.CreateTokenAsync(loginDto);
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

        var token = await _authenticationManager.GetEmailConfirmationLinkAsync(user);
        await _emailSender.SendEmailConfirmationEmail(user, token);
    }
    
    public async Task ConfirmEmail(string email, string token)
    {
        var result = await _authenticationManager.ConfirmEmailAsync(email, token);
        if (!result)
            throw new CommonErrorException(400, "Failed confirming email", 0);
    }

    public async Task<bool> CheckIfEmailIsConfirmed(string email)
    {
        try
        {
            return await _authenticationManager.IsEmailConfirmed(email);
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<string> VerifyReCaptcha(string userToken)
    {
        var baseUrl = "https://www.google.com/recaptcha/api/siteverify";
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["secret"] = _configuration["ReCaptchaSecret"];
        queryParams["response"] = userToken;

        var requestUrl = baseUrl + "?" + queryParams.ToString();
        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.PostAsync(requestUrl, null);
        return response.Content.ReadAsStringAsync().Result;
    }
}