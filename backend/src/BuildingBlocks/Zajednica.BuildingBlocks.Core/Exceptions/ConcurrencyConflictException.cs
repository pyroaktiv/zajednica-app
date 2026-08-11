namespace Zajednica.BuildingBlocks.Core.Exceptions;

public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string message) : base(message) { }

    public ConcurrencyConflictException(string message, Exception? innerException) : base(message, innerException) { }
}
