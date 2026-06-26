namespace ProcureHub.SharedKernel.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string entityName, string field, string value)
        : base($"{entityName} with {field} '{value}' already exists.") { }

    public ConflictException(string message) : base(message) { }
}
