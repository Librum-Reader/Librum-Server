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

    [HttpPut("{guid:guid}")]
    public async Task<ActionResult> UpdateBlog([FromBody] BlogInDto blogInDto,
                                               Guid guid)
    {
        return Ok();
    }

    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> DeleteBlog(Guid guid)
    {
        try
        {
            await _blogService.DeleteBlogAsync(guid);
            return NoContent();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    
    [HttpPost("content/{guid:guid}")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(10485760)]   // Allow max 10MiB
    public async Task<ActionResult> AddBlogContent(Guid guid)
    {
        // Check if the data was sent in the correct format
        var isMultiPart = !string.IsNullOrEmpty(Request.ContentType) &&
                          Request.ContentType.IndexOf(
                              "multipart/",
                              StringComparison.OrdinalIgnoreCase) >= 0;
        if (!isMultiPart)
        {
            var message = "The blog content needs to be a multipart";
            return StatusCode(400, new CommonErrorDto(400, message, 0));
        }
        
        var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType));
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
    
        try
        {
            await _blogService.AddBlogContent(guid, reader);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    
        return Ok();
    }
    
    [HttpGet("content/{guid:guid}")]
    public async Task<ActionResult> GetBlogContent(Guid guid)
    {
        try
        {
            var stream = await _blogService.GetBlogContent(guid);
            return File(stream, "application/octet-stream");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    
    [HttpPost("cover/{guid:guid}")]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(5242880)]   // Allow max 5MB
    public async Task<ActionResult> ChangeCoverPicture(Guid guid)
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
            // await _blogService.ChangeCover(guid, reader);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    
        return Ok();
    }
    
    [HttpGet("cover/{guid:guid}")]
    public async Task<ActionResult> GetCover(Guid guid)
    {
        try
        {
            var stream = await _blogService.GetCover(guid);
            return File(stream, "application/octet-stream");
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpDelete("cover/{guid:guid}")]
    public async Task<ActionResult> DeleteCover(Guid guid)
    {
        try
        {
            await _blogService.DeleteCover(guid);
            return NoContent();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
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
}