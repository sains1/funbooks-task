namespace SharedKernel.GenericResponses;

public class ConflictResponse
{
    private ConflictResponse()
    {
    }

    public static readonly ConflictResponse Value = new();
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}