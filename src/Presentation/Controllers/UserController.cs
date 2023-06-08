using Application.Common.Attributes;
using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;


    public UserController(IUserService userService, 
                          ILogger<UserController> logger)
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
    public async Task<ActionResult> PatchUser(
        JsonPatchDocument<UserForUpdateDto> patchDoc)
    {
        try
        {
            await _userService.PatchUserAsync(HttpContext.User.Identity!.Name,
                                              patchDoc, this);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        await _userService.DeleteUserAsync(HttpContext.User.Identity!.Name);
        return NoContent();
    }
    
    [HttpPost("profilePicture")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(5242880)]   // Allow max 5MB
    public async Task<ActionResult> ChangeProfilePicture()
    {
        // Check if the profile picture was sent in the correct format
        var isMultiPart = !string.IsNullOrEmpty(Request.ContentType) &&
                          Request.ContentType.IndexOf(
                              "multipart/",
                              StringComparison.OrdinalIgnoreCase) >= 0;
        if (!isMultiPart)
        {
            var message = "The profile picture needs to be a multipart";
            return StatusCode(400, new CommonErrorDto(400, message, 0));
        }
        
        var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
    
        try
        {
            await _userService.ChangeProfilePicture(HttpContext.User.Identity!.Name,
                                                    reader);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    
        return Ok();
    }
    
    private static string GetBoundary(MediaTypeHeaderValue contentType)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        return boundary;
    }
    
    [HttpGet("profilePicture")]
    public async Task<ActionResult> GetProfilePicture(Guid guid)
    {
        try
        {
            var stream = await _userService.GetProfilePicture(HttpContext.User.Identity!.Name);
            
            Response.Headers.Add("Guid", guid.ToString());
            return File(stream, "application/octet-stream");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpDelete("profilePicture")]
    public async Task<ActionResult> DeleteProfilePicture()
    {
        try
        {
            await _userService.DeleteProfilePicture(HttpContext.User.Identity!.Name);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }

        return Ok();
    }
}