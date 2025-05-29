using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Extensions;
using Application.Common.Exceptions;
using Application.Interfaces.Repositories;
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
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IConfiguration _configuration;

    public AiService(ILogger<AiService> logger, IHttpClientFactory httpClientFactory,
                     IUserRepository userRepository, IProductRepository productRepository, IConfiguration configuration)
    {
        _logger = logger;
        
        _httpClientFactory = httpClientFactory;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _configuration = configuration;
    }

    public async Task ExplainAsync(string email, HttpContext context, string text, 
                                   string mode)
    {
		//  The OpenAIToken needs to be provided by the user when the server is self-hosted
	    if (_configuration["LIBRUM_SELFHOSTED"] == "true" && String.IsNullOrEmpty(_configuration["OpenAIToken"]))
        {
            throw new CommonErrorException(403, "Ai explanation is unavailable when selfhosting Librum. " +
                                                "You will need to provide an OpenAI Token yourself.", 20);
        }
	
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var aiRequestLimit = (await _productRepository.GetByIdAsync(user.ProductId)).AiRequestLimit;
        if(user.AiExplanationRequestsMadeToday >= aiRequestLimit)
        {
            const string message = "Ai explanation limit reached";
            _logger.LogWarning(message);
            throw new CommonErrorException(403, message, 26);
        }
        
        await context.SSEInitAsync();
        
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
            const string message = "Ai explanation failed";
            _logger.LogWarning(message);
            throw new CommonErrorException(400, message, 0);
        }
        
        // Increment the ai explanation count on the user
        user.AiExplanationRequestsMadeToday++;
        await _userRepository.SaveChangesAsync();
        
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
        return finishReason != null;
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

    public async Task<string> TranslateAsync(string email, string text, string sourceLang, string targetLang) {
        //  The DeepL Token needs to be provided by the user when the server is self-hosted
	    if (_configuration["LIBRUM_SELFHOSTED"] == "true" && String.IsNullOrEmpty(_configuration["DeepLToken"]))
        {
            throw new CommonErrorException(403, "DeepL translation is unavailable when selfhosting Librum. " +
                                                "You will need to provide a DeepL Token yourself.", 20);
        }
	
        var user = await _userRepository.GetAsync(email, trackChanges: true);
        var translationLimit = (await _productRepository.GetByIdAsync(user.ProductId)).TranslationsLimit;
        if(user.AiExplanationRequestsMadeToday >= translationLimit)
        {
            const string message = "Translation limit reached";
            _logger.LogWarning(message);
            throw new CommonErrorException(403, message, 26);
        }
        
        
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Librum/1.0.0");
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",  _configuration["DeepLToken"]);

        var request = CreateDeeplTranslationRequest(email, text, sourceLang, targetLang);
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            const string message = "DeepL translation failed";
            _logger.LogWarning(message);
            throw new CommonErrorException(400, message, 0);
        }

        var content = await response.Content.ReadAsStringAsync();
        var jsonObj = JsonConvert.DeserializeObject<dynamic>(content);
        var result = jsonObj["translations"][0]["text"].Value.ToString();
        
        // Increment the translation count on the user
        user.TranslationRequestsMadeToday++;
        await _userRepository.SaveChangesAsync();

        return result;
    }

    private HttpRequestMessage CreateDeeplTranslationRequest(string email,
                                                             string text,
                                                             string sourceLang,
                                                             string targetLang)
    {
        var url = "https://api-free.deepl.com/v2/translate";
        var body = new
        {
            text = new List<dynamic>
            {
                text
            },
            source_lang = sourceLang,
            target_lang = targetLang,
            split_sentences = "0"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(body);

        return request;
    }
}