using Zajednica.BuildingBlocks.Core.Domain;

namespace Zajednica.Community.Core.Domain;

public class MembershipRole : Entity
{
    public CommunityRole Role { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public Guid? GrantedByMembershipId { get; private set; }
    
    private MembershipRole() { }

    internal MembershipRole(CommunityRole role, Guid? grantedByMembershipId, DateTime grantedAt)
    {
        Role = role;
        GrantedByMembershipId = grantedByMembershipId;
        GrantedAt = grantedAt;
    }
}
