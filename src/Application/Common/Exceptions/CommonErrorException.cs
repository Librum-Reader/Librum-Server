using Application.Common.DTOs;

namespace Application.Common.Exceptions;

public class CommonErrorException : Exception
{
    public CommonErrorDto Error { get; }
    
    public CommonErrorException(int status, string message, int code)
        : base(message)
    {
        Error = new CommonErrorDto(status, message, code);
    }
}