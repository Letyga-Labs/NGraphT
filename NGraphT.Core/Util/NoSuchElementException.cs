namespace NGraphT.Core.Util;

public sealed class NoSuchElementException : Exception
{
    public NoSuchElementException()
    {
    }

    public NoSuchElementException(string message)
        : base(message)
    {
    }

    public NoSuchElementException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
