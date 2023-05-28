using Newtonsoft.Json;

namespace Application.Common.DTOs;

/// <summary>
/// Common errors such as "Wrong Password
/// </summary>
public class CommonErrorDto
{
    public int Status { get; set; }
    public string Message { get; set; }

    public CommonErrorDto(int status, string message)
    {
        Status = status;
        Message = message;
    }
    
    public override string ToString() => 
        JsonConvert.SerializeObject(this, new JsonSerializerSettings());
}