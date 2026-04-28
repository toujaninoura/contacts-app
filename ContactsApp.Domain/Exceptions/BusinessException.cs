namespace ContactsApp.Domain.Exceptions;

public class BusinessException : AppException
{
    public BusinessException(string message)
        : base(message, 422) { }
}
