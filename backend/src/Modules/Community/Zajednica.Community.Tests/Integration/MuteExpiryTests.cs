using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Core.UseCases;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class MuteExpiryTests : BaseCommunityIntegrationTest
{
    public MuteExpiryTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public void Only_a_mute_whose_window_has_passed_is_ended()
    {
        using var scope = Factory.Services.CreateScope();
        var issuerId = NewAccount(scope);
        var expiredId = NewAccount(scope);
        var stillMutedId = NewAccount(scope);
        var community = CreateCommunity(scope, issuerId);
        var qrToken = QrToken(scope, issuerId, community.Id);
        Join(scope, expiredId, qrToken);
        Join(scope, stillMutedId, qrToken);
        var expired = Certify(scope, issuerId, expiredId, community.Id);
        var stillMuted = Certify(scope, issuerId, stillMutedId, community.Id);

        scope.ServiceProvider.GetRequiredService<IInternalIntentOutcomeService>().Mute(stillMuted.MembershipId);

        var db = Db(scope);
        db.Memberships.Single(m => m.Id == expired.MembershipId).Mute(DateTime.UtcNow.AddDays(-4));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        scope.ServiceProvider.GetRequiredService<MuteExpiry>().EndExpired();

        db.ChangeTracker.Clear();
        db.Memberships.Single(m => m.Id == expired.MembershipId).MutedUntil.ShouldBeNull();
        db.Memberships.Single(m => m.Id == stillMuted.MembershipId).MutedUntil.ShouldNotBeNull();
    }
}
