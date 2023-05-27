namespace Application.Common.Exceptions;

public class StorageLimitExceededException : Exception
{
    public StorageLimitExceededException(string message) 
        : base(message)
    {
    }
}