using Application.Common.DTOs.Folders;
using Application.Common.Exceptions;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FolderController(IFolderService folderService, ILogger<FolderController> logger) : ControllerBase
{
    private IFolderService FolderService { get; } = folderService;
    private ILogger<FolderController> Logger { get; } = logger;

    [HttpGet]
    public async Task<IActionResult> GetFolders()
    {
        try
        {
            var email = HttpContext.User.Identity!.Name;
            var folders = await FolderService.GetFoldersAsync(email);
            return Ok(folders);
        }
        catch (CommonErrorException e)
        {
            Logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
    
    [HttpPost("update")]
    public async Task<IActionResult> UpdateFolders([FromBody] FolderInDto folderInDto)
    {
        try
        {
            var email = HttpContext.User.Identity!.Name;
            await FolderService.UpdateFoldersAsync(email, folderInDto);
            return Ok();
        }
        catch (CommonErrorException e)
        {
            Logger.LogWarning("{ErrorMessage}", e.Message);
            return StatusCode(e.Error.Status, e.Error);
        }
    }
}