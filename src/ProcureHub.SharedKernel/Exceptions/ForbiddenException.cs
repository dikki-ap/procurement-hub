namespace ProcureHub.SharedKernel.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.") { }
}
