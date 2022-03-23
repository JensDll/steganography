namespace Domain.Exceptions;

public class MessageTooLongException : Exception
{
    public MessageTooLongException(string message) : base(message)
    {
    }
}
