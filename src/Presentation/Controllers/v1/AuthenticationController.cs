using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[ApiController]
[Route("api")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;


    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null)
        {
            _logger.LogWarning("User registration failed due to the register dto being null");
            return BadRequest("The provided data was invalid");
        }

        try
        {
            await _authenticationService.RegisterUserAsync(registerDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("User registration failed: " + e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<string>> LoginUser([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
        {
            _logger.LogWarning("User login failed due to the register dto being null");
            return BadRequest("The provided data was invalid");
        }

        try
        {
            return await _authenticationService.LoginUserAsync(loginDto);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("User registration failed: " + e.Message);
            return BadRequest(e.Message);
        }
    }

    [AllowAnonymous]
    [HttpPost("recoverAccount/{email}")]
    public async Task<ActionResult> RecoverAccount(string email)
    {
        return Ok();
    }
}