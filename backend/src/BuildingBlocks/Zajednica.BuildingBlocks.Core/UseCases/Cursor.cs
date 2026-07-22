using System.Text;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.BuildingBlocks.Core.UseCases;

public sealed record Cursor(DateTime Date, Guid Id)
{
    public string Encode() => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Date.Ticks}:{Id}"));

    public static Cursor? Decode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(value)).Split(':');
            return new Cursor(new DateTime(long.Parse(parts[0]), DateTimeKind.Utc), Guid.Parse(parts[1]));
        }
        catch (Exception)
        {
            throw new EntityValidationException("Malformed cursor.");
        }
    }
}
