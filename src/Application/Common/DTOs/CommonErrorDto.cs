using Newtonsoft.Json;

namespace Application.Common.DTOs;

/// <summary>
/// Common errors such as "Wrong Password
/// </summary>
public class CommonErrorDto
{
    public int Status { get; set; }
    public string Message { get; set; }
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