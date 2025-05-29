using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IAiService
{
    Task ExplainAsync(string email, HttpContext context, string text, string mode);
    Task<string> TranslateAsync(string email, string text, string sourceLang, string targetLang);
}