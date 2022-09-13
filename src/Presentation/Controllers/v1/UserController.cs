using Application.Common.ActionFilters;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ServiceFilter(typeof(ValidateUserExistsAttribute))]
[ServiceFilter(typeof(ValidateStringParameterAttribute))]
[ApiController]
[ApiVersion("1.0")]
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
        var user = await _userService.GetUserAsync(HttpContext.User.Identity!.Name);
        return Ok(user);
    }

    [HttpPatch]
    public async Task<ActionResult> PatchUser([FromBody] JsonPatchDocument<UserForUpdateDto> patchDoc)
    {
        try
        {
            await _userService.PatchUserAsync(HttpContext.User.Identity!.Name, patchDoc, this);
            return Ok();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        await _userService.DeleteUserAsync(HttpContext.User.Identity!.Name);
        return NoContent();
    }
}