using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;


    public AuthenticationController(IAuthenticationService authenticationService,
                                    ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterDto registerDto)
    {
        try
        {
            await _authenticationService.RegisterUserAsync(registerDto);
            return StatusCode(201);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<string>> LoginUser([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authenticationService.LoginUserAsync(loginDto);
            return Ok(result);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [AllowAnonymous]
    [HttpGet("confirmEmail")]
    public async Task<ContentResult> ConfirmEmail(string email, string token)
    {
        try
        {
            await _authenticationService.ConfirmEmail(email, token);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                                        "wwwroot",
                                        "EmailConfirmationSucceeded.html");
            var successContent = await System.IO.File.ReadAllTextAsync(filePath);

            return base.Content(successContent, "text/html");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                                        "wwwroot",
                                        "EmailConfirmationFailed.html");
            var successContent = await System.IO.File.ReadAllTextAsync(filePath);
            return base.Content(successContent, "text/html");
        }
    }

    [AllowAnonymous]
    [HttpGet("checkIfEmailConfirmed/{email}")]
    public async Task<ActionResult<bool>> CheckIfEmailIsConfirmed(string email)
    {
        var confirmed = await _authenticationService.CheckIfEmailIsConfirmed(email);
        return Ok(confirmed);
    }

    [AllowAnonymous]
    [HttpPost("recoverAccount/{email}")]
    public ActionResult RecoverAccount(string email)
    {
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("recaptchaVerify")]
    public async Task<ActionResult> ReCaptchaVerify(string userToken)
    {
        var result = await _authenticationService.VerifyReCaptcha(userToken);

        return Ok(result);
    }
}