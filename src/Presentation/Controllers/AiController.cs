using Application.Common.DTOs;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AiController : ControllerBase
{
    private readonly ILogger<AiController> _logger;
    private readonly IAiService _aiService;

    public AiController(ILogger<AiController> logger, IAiService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    public struct ExplainRequest
    {
        public string Text { get; set; }
        public string Mode { get; set; }
    }

    public struct TranslateRequest
    {
        public string Text { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
    }
    
    [HttpPost("complete")]
    public async Task<ActionResult> Explain(ExplainRequest request)
    {
        if (request.Text.Length > 5000)
        {
            const string message = "The text is too long";
            _logger.LogWarning(message);
            return StatusCode(400, new CommonErrorDto(400, message, 26));
        }
        
        try
        {
            await _aiService.ExplainAsync(HttpContext.User.Identity!.Name, HttpContext,
                                          request.Text, request.Mode);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            return StatusCode(e.Error.Status, e.Error);
        }
    }

    [HttpPost("translate")]
    public async Task<ActionResult<string>> Translate(TranslateRequest request)
    {
        if (request.Text.Length > 3000)
        {
            const string message = "The text is too long";
            _logger.LogWarning(message);
            return StatusCode(400, new CommonErrorDto(400, message, 27));
        }
        
        try
        {
            var result = await _aiService.TranslateAsync(HttpContext.User.Identity!.Name, request.Text, 
                                                         request.SourceLang, request.TargetLang);
            return Ok(result);
        }
        catch (CommonErrorException e)
        {
            return StatusCode(e.Error.Status, e.Error);
        }
    }
}