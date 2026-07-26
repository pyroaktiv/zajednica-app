using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Community.Core.Domain;

public class RoleGrant : Entity
{
    public CommunityRole Role { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public Guid? GrantedByMembershipId { get; private set; }
    
    private RoleGrant() { }

    internal RoleGrant(CommunityRole role, Guid? grantedByMembershipId, DateTime grantedAt)
    {
        Role = role;
        GrantedByMembershipId = grantedByMembershipId;
        GrantedAt = grantedAt;
    }
}
