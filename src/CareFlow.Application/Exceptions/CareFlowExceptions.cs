namespace CareFlow.Application.Exceptions;

public abstract class CareFlowException : Exception
{
    protected CareFlowException(string message) : base(message) { }
}

public class NotFoundException : CareFlowException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, int id) : base($"{entity} with ID {id} was not found.") { }
}

public class ValidationException : CareFlowException
{
    public List<string> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(IEnumerable<string> errors) : base("Validation failed.")
    {
        Errors = errors.ToList();
    }
}

public class ConflictException : CareFlowException
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : CareFlowException
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
}

public class ForbiddenException : CareFlowException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(message) { }
}
