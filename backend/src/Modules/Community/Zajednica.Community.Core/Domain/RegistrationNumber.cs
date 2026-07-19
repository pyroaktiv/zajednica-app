using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

// MB (maticni broj) - exactly 8 digits.
public class RegistrationNumber : ValueObject
{
    public string Value { get; private set; } = null!;

    // EF
    private RegistrationNumber() { }

    public RegistrationNumber(string value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length != 8 || !trimmed.All(char.IsDigit))
            throw new EntityValidationException("Registration number (MB) must be exactly 8 digits.");
        Value = trimmed;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
