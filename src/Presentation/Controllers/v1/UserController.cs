using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Presentation.Controllers.v1;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;


    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    
    [HttpGet]
    public async Task<ActionResult<UserOutDto>> GetUser()
    {
        try
        {
            return await _userService.GetUserAsync(HttpContext.User.Identity.Name);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Getting user: " + e.Message);
            return BadRequest(e.Message);
        }
    }
}