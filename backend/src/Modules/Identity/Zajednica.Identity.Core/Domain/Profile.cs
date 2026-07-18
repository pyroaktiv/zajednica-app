using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Identity.Core.Domain;

/// <summary>
/// Optional personal data, entity inside the Account aggregate. Its lifecycle is owned by
/// <see cref="Account"/> (created / updated / removed only through the root). All fields are
/// optional; a blank value is stored as null so <see cref="HasManagerData"/> is unambiguous.
/// </summary>
public class Profile : Entity
{
    public string? ImageUrl { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    // EF
    private Profile() { }

    internal Profile(string? firstName, string? lastName, string? phone, string? contactEmail, string? imageUrl)
        => Update(firstName, lastName, phone, contactEmail, imageUrl);

    internal void Update(string? firstName, string? lastName, string? phone, string? contactEmail, string? imageUrl)
    {
        FirstName = Clean(firstName);
        LastName = Clean(lastName);
        Phone = Clean(phone);
        Email = Clean(contactEmail);
        ImageUrl = Clean(imageUrl);
    }

    /// <summary>Manager candidacy requires first name, last name and phone.</summary>
    public bool HasManagerData() => FirstName is not null && LastName is not null && Phone is not null;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
