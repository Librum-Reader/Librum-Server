using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api")]
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
    [HttpGet]
    public ActionResult Get()
    {
        return Ok("Librum-Api Version 1");
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
    [HttpGet("confirmEmail/{email}")]
    public async Task<ActionResult> ConfirmEmail(string email, [FromBody] string token)
    {
        try
        {
            await _authenticationService.ConfirmEmail(email, token);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [AllowAnonymous]
    [HttpPost("recoverAccount/{email}")]
    public ActionResult RecoverAccount(string email)
    {
        return Ok();
    }
}