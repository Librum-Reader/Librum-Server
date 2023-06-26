using Newtonsoft.Json;

namespace Application.Common.DTOs;

/// <summary>
/// Common errors such as "Wrong Password
/// </summary>
public class CommonErrorDto
{
    [JsonProperty("status")]
    public int Status { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("code")]
    public int Code { get; set; }

    public CommonErrorDto(int status, string message, int code)
    {
        Status = status;
        Message = message;
        Code = code;
    }
    
    public override string ToString() => 
        JsonConvert.SerializeObject(this, new JsonSerializerSettings());
}