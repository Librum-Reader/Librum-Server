using Application.Common.Extensions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    public struct ExplainRequest
    {
        public string Text { get; set; }
        public string Mode { get; set; }
    }
    
    [HttpPost("complete")]
    public async Task Explain(ExplainRequest request)
    {
        await HttpContext.SSEInitAsync();
        
        await _aiService.ExplainAsync(HttpContext.User.Identity!.Name, HttpContext,
                                      request.Text, request.Mode);
    }
}