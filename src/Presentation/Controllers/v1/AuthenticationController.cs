using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
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
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterDto registerDto)
    {
        try
        {
            await _authenticationService.RegisterUserAsync(registerDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);
            return BadRequest(e.Message);
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
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ExceptionMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [AllowAnonymous]
    [HttpPost("recoverAccount/{email}")]
    public ActionResult RecoverAccount(string email)
    {
        return Ok();
    }
}