using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagController> _logger;


    public TagController(ITagService tagService, ILogger<TagController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }


    [HttpPost("create")]
    public async Task<ActionResult> CreateTag([FromBody] TagInDto tagInDto)
    {
        if (tagInDto == null)
        {
            _logger.LogWarning("Creating tag failed: The provided tag dto is null");
            return BadRequest("The provided data is invalid");
        }
    
        try
        {
            await _tagService.CreateTagAsync(HttpContext.User.Identity!.Name, tagInDto);
            return StatusCode(201);
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Creating tag failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete("{tagName}")]
    public async Task<ActionResult> DeleteTag(string tagName)
    {
        try
        {
            await _tagService.DeleteTagAsync(HttpContext.User.Identity!.Name, tagName);
            return NoContent();
        }
        catch (InvalidParameterException e)
        {
            _logger.LogWarning("Creating tag failed: {ErrorMessage}", e.Message);
            return BadRequest(e.Message);
        }
    }
}