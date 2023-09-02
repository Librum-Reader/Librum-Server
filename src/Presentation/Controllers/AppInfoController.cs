using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Presentation.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;


    public AppInfoController(ILogger<UserController> logger,
                             IHttpClientFactory httpClientFactory,
                             IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    [AllowAnonymous]
    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("latest-version")]
    public async Task<string> GetLatestVersion()
    {
        var url = "https://api.github.com/repos/Librum-Reader/Librum/releases";
        
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Librum/1.0.0");
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _configuration["GitAccessToken"]);

        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            string responseBody = await response.Content.ReadAsStringAsync();;

            var jsonList = JsonConvert.DeserializeObject<List<dynamic>>(responseBody);
            string name = jsonList.First().name;
            
            name = name[2..]; // Remove "v." from version
            return name;
        }

        _logger.LogWarning("Getting latest application version failed");
        return "0";
    }
}