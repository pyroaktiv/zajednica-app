namespace Zajednica.BuildingBlocks.Core.Exceptions;

public class EntityValidationException : DomainException
{
    public EntityValidationException(string message) : base(message) {}
}