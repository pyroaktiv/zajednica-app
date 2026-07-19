using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

// PIB (poreski identifikacioni broj) - exactly 9 digits.
public class TaxId : ValueObject
{
    public string Value { get; private set; } = null!;

    // EF
    private TaxId() { }

    public TaxId(string value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length != 9 || !trimmed.All(char.IsDigit))
            throw new EntityValidationException("Tax id (PIB) must be exactly 9 digits.");
        Value = trimmed;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
