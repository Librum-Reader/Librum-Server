using Application.Common.DTOs;
using Application.Common.DTOs.User;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
            return await _userService.GetUserAsync(HttpContext.User.Identity!.Name);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Getting user failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpPatch]
    public async Task<ActionResult> PatchUser([FromBody] JsonPatchDocument<UserForUpdateDto> patchDoc)
    {
        if (patchDoc == null)
        {
            _logger.LogWarning("Patching user failed: The provided JsonPatchDocument is null");
            return BadRequest("The provided data is invalid");
        }
    
        try
        {
            await _userService.PatchUserAsync(HttpContext.User.Identity!.Name, patchDoc, this);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Patching user failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        try
        {
            await _userService.DeleteUserAsync(HttpContext.User.Identity!.Name);
            return NoContent();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Deleting user failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}