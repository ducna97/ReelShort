namespace ReelShort.Application.Common.Exceptions;

public sealed class ExternalAuthException : Exception
{
    public ExternalAuthException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
