using Application.Common.DTOs;

namespace Application.Common.Exceptions;

public class CommonErrorException : Exception
{
    public CommonErrorDto Error { get; }
    
    public CommonErrorException(int statusCode, string message)
        : base(message)
    {
        Error = new CommonErrorDto(statusCode, message);
    }
}