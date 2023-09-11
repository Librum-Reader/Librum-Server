using Application.Common.DTOs.Highlights;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HighlightController : ControllerBase
{
    private readonly IHighlightService _highlightService;
    private readonly ILogger<HighlightController> _logger;


    public HighlightController(IHighlightService highlightService,
                               ILogger<HighlightController> logger)
    {
        _highlightService = highlightService;
        _logger = logger;
    }
    
    [HttpPost("{guid:guid}")]
    public async Task<ActionResult> CreateHighlight(Guid guid,
                                                    [FromBody] HighlightInDto highlightIn)
    {
        try
        {
            await _highlightService.CreateHighlightAsync(User.Identity!.Name, guid, 
                                                         highlightIn);
            return StatusCode(201);
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpDelete("{bookGuid:guid}/{highlightGuid:guid}")]
    public async Task<ActionResult> DeleteHighlight(Guid bookGuid, Guid highlightGuid)
    {
        try
        {
            await _highlightService.DeleteHighlightAsync(User.Identity!.Name, bookGuid, 
                                                         highlightGuid);
            return NoContent();
        }
        catch (CommonErrorException e)
        {
            _logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
}