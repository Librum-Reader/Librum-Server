using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagController> _logger;


    public TagController(ILogger<TagController> logger, ITagService tagService)
    {
        _tagService = tagService;
        _logger = logger;
    }
    
    
    [HttpDelete("{guid:guid}")]
    public async Task<ActionResult> DeleteTag(Guid guid)
    {
        await _tagService.DeleteTagAsync(HttpContext.User.Identity!.Name, guid);
        return NoContent();
    }
    
    [HttpPut]
    public async Task<ActionResult> UpdateTag(TagForUpdateDto tagUpdateDto)
    {
        try
        {
            await _tagService.UpdateTagAsync(HttpContext.User.Identity!.Name,
                                             tagUpdateDto);
            return StatusCode(201);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagOutDto>>> GetTags()
    {
        var userName = HttpContext.User.Identity!.Name;
        var result = await _tagService.GetTagsAsync(userName);
        return Ok(result);
    }
}