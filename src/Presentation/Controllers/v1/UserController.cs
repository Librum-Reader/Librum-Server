using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Presentation.Controllers.v1;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;


    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    
    [HttpGet]
    public async Task<ActionResult<UserOutDto>> GetUser()
    {
        try
        {
            return await _userService.GetUserAsync("test");
        }
        catch (InvalidParameterException e)
        {
            return BadRequest(e.Message);
        }
    }
}