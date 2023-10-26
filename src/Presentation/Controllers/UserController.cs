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
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
	private readonly IConfiguration _configuration;


    public UserController(IUserService userService, 
                          ILogger<UserController> logger,
						  IConfiguration configuration
						  )
    {
        _userService = userService;
        _logger = logger;
		_configuration = configuration;
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
    
    public class StringInputModel
    {
        public string Input { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult> ChangePassword([FromBody] StringInputModel inputModel)
    {
        try
        {
            await _userService.ChangePasswordAsync(HttpContext.User.Identity!.Name,
                                                   inputModel.Input);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    public class PasswordResetModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }
	
	// ---------------------------
	// added HttpGet method for self-hosted version to show simple form for password change
	// it's needed because we can't go to librum.com when self-hosted
	[AllowAnonymous]
	[HttpGet("resetPassword")]
	public async Task<ActionResult> ResetPasswordPage(string email,string token){
		var content ="<!DOCTYPE html><html><head><title>Reset Password</title></head>"+
						"<body><h1>Reset Password</h1><form action=\"/user/resetPassword\" "+
						"method=\"post\" style=\"margin:auto; display:grid;max-width:200px;\">"+
						"<input type=\"hidden\"  name=\"Email\" value=\""+email+"\"><br><br>"+
						"<input type=\"hidden\"  name=\"Token\" value=\""+token+"\"><br>"+
						"<input type=\"password\" name=\"Password\" minlength=5 required><br>"+
    					"<input type=\"submit\" value=\"Reset Password\"></form></body></html>";
		
		return new ContentResult()
   		{
        	Content = content,
        	ContentType = "text/html",
    	};				
						
	}
	// ----------------------------------------------
	// end of HttpGet added 
	
    [AllowAnonymous]
    [HttpPost("resetPassword")]
    public async Task<ActionResult> ResetPasswordWithToken([FromBody] PasswordResetModel model)
    {
        try
        {
			// in self hosted version need to replace spaces with "+" to work normally
			var token=model.Token;
			if (_configuration["LIBRUM_SELFHOSTED"] == "true"){
				token =System.Web.HttpUtility.HtmlDecode( model.Token.Replace(" ","+") );
			}
            await _userService.ChangePasswordWithTokenAsync(model.Email, token, model.Password);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [AllowAnonymous]
    [HttpPost("forgotPassword/{email}")]
    public async Task<ActionResult> ForgotPassword(string email)
    {
        await _userService.ForgotPassword(email);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteUser()
    {
        try
        {
            await _userService.DeleteUserAsync(HttpContext.User.Identity!.Name);
            return NoContent();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
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
            if (stream.Position == stream.Length)
                return StatusCode(400, "No profile picture exists");
            
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