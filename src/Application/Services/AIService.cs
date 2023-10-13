using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Application.Services;

public class AiService : IAiService
{
    private readonly ILogger<AiService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiService(ILogger<AiService> logger, IHttpClientFactory httpClientFactory,
                     IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task ExplainAsync(string email, HttpContext context, string text, 
                                   string mode)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Librum/1.0.0");

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _configuration["OpenAIToken"]);

        var request = CreateOpenAiExplanationRequest(email, mode, text);
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("AI explanation failed");
            return;
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var currentEvent = new StringBuilder();
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line.IsNullOrEmpty() && !currentEvent.ToString().IsNullOrEmpty())
            {
                var str = currentEvent.ToString();
                var fixedStr = str.Remove(0, "data: ".Length);
                var jsonObj = JsonConvert.DeserializeObject<dynamic>(fixedStr);

                if (IsLastMessage(jsonObj))
                    break;
                
                var firstChoice = jsonObj["choices"][0];
                
                string message = firstChoice["delta"]["content"].ToString();
                await context.Response.WriteAsync("data: " + message + "\n");
                await context.Response.WriteAsync("\n");
                await context.Response.Body.FlushAsync();
                
                currentEvent.Clear();
            }
            else
            {
                currentEvent.AppendLine(line);
            }
        }
    }

    private HttpRequestMessage CreateOpenAiExplanationRequest(string email, 
                                                              string mode, 
                                                              string text)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        var body = new
        {
            model = "gpt-3.5-turbo",
            messages = new List<dynamic>
            {
                new
                {
                    role = "user",
                    content = mode + "\"" + text + "\""
                }
            },
            temperature = 0.1,
            max_tokens = 280,
            user = GetHashString(email),
            stream = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(body);

        return request;
    }

    private bool IsLastMessage(dynamic jsonObj)
    {
        var firstChoice = jsonObj["choices"][0];

        var finishReason = firstChoice["finish_reason"];
        return finishReason == null;
    }
    
    private static byte[] GetHash(string inputString)
    {
        using HashAlgorithm algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }
}