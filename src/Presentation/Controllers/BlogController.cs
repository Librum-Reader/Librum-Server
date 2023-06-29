using Application.Common.Attributes;
using Application.Common.DTOs;
using Application.Common.DTOs.Blogs;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Presentation.Controllers;

// [Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;
    private readonly ILogger<BlogController> _logger;


    public BlogController(IBlogService blogService, 
                          ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> CreateBlog([FromBody] BlogInDto blogInDto)
    {
        try
        {
            await _blogService.CreateBlogAsync(blogInDto);
            return StatusCode(201);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [HttpGet]
    public ActionResult<ICollection<BlogOutDto>> GetBlogs()
    {
        return Ok(_blogService.GetAllBlogs());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateBlog([FromBody] BlogInDto blogInDto, Guid id)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteBlog(Guid id)
    {
        try
        {
            await _blogService.DeleteBlogAsync(id);
            return NoContent();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpPost("cover")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(5242880)]   // Allow max 5MB
    public async Task<ActionResult> ChangeCoverPicture()
    {
        // Check if the profile picture was sent in the correct format
        var isMultiPart = !string.IsNullOrEmpty(Request.ContentType) &&
                          Request.ContentType.IndexOf(
                              "multipart/",
                              StringComparison.OrdinalIgnoreCase) >= 0;
        if (!isMultiPart)
        {
            var message = "The cover needs to be a multipart";
            return StatusCode(400, new CommonErrorDto(400, message, 0));
        }
        
        var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
    
        try
        {
            ;
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
    
    [HttpGet("cover")]
    public async Task<ActionResult> GetCover(Guid guid)
    {
        try
        {
            return Ok();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
}