namespace Zajednica.Community.Api.Internal;

public interface IInternalStarAwardService
{
    void AddStars(Guid membershipId, int stars);
}
