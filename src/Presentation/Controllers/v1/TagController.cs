using Application.Common.ActionFilters;
using Application.Common.DTOs.Tags;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.v1;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;


    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }
    
    
    [HttpDelete("{guid:guid}")]
    [TypeFilter(typeof(TagExistsFilter))]
    public async Task<ActionResult> DeleteTag(Guid guid)
    {
        await _tagService.DeleteTagAsync(HttpContext.User.Identity!.Name, guid);
        return NoContent();
    }
    
    [HttpPut]
    [TypeFilter(typeof(TagExistsFilter))]
    public async Task<ActionResult> UpdateTag(TagForUpdateDto tagUpdateDto)
    {
        await _tagService.UpdateTagAsync(HttpContext.User.Identity!.Name,
                                         tagUpdateDto);
        return StatusCode(201);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagOutDto>>> GetTags()
    {
        var userName = HttpContext.User.Identity!.Name;
        var result = await _tagService.GetTagsAsync(userName);
        return Ok(result);
    }
}