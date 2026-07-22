using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Identity.Core.Domain;

public class Profile : Entity
{
    public string? ImageUrl { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

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

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
