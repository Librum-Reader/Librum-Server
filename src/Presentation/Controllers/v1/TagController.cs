using Application.Common.ActionFilters;
using Application.Common.DTOs.Tags;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ServiceFilter(typeof(ValidateUserExistsAttribute))]
[ServiceFilter(typeof(ValidateStringParameterAttribute))]
[ApiController]
[ApiVersion("1.0")]
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
    [TypeFilter(typeof(ValidateTagDoesNotExistAttribute))]
    public async Task<ActionResult> CreateTag([FromBody] TagInDto tagInDto)
    {
        await _tagService.CreateTagAsync(HttpContext.User.Identity!.Name, tagInDto);
        return StatusCode(201);
    }
    
    [HttpDelete("{tagName}")]
    [ServiceFilter(typeof(ValidateTagExistsAttribute))]
    public async Task<ActionResult> DeleteTag(string tagName)
    {
        await _tagService.DeleteTagAsync(HttpContext.User.Identity!.Name, tagName);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagOutDto>>> GetTags()
    {
        var result = await _tagService.GetTagsAsync(HttpContext.User.Identity!.Name);
        return Ok(result);
    }
}