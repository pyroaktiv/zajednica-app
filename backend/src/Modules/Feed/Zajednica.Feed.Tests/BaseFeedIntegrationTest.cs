using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Api.Controllers.Feed;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Feed.Api.Public;
using Zajednica.Feed.Infrastructure.Database;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Feed.Tests;

public class BaseFeedIntegrationTest : BaseWebIntegrationTest<FeedTestFactory>
{
    public BaseFeedIntegrationTest(FeedTestFactory factory) : base(factory) { }

    protected sealed record Member(Guid AccountId, Guid MembershipId);

    protected static FeedDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<FeedDbContext>();

    protected static CommunityDbContext CommunityDb(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<CommunityDbContext>();

    protected static PostController Posts(IServiceScope scope, Guid accountId) =>
        As(new PostController(scope.ServiceProvider.GetRequiredService<IPostService>()), accountId);

    protected static CommentController Comments(IServiceScope scope, Guid accountId) =>
        As(new CommentController(scope.ServiceProvider.GetRequiredService<ICommentService>()), accountId);

    protected static IntentController Intents(IServiceScope scope, Guid accountId) =>
        As(new IntentController(
            scope.ServiceProvider.GetRequiredService<IIntentCommandService>(),
            scope.ServiceProvider.GetRequiredService<IIntentQueryService>()), accountId);

    protected static (CommunityDetailsDto Community, Member Owner) CreateCommunity(IServiceScope scope)
    {
        var accountId = NewAccount(scope);
        var request = new CreateCommunityRequestDto(
            $"Zgrada {Guid.NewGuid():N}", new AddressDto("Bulevar", "12", 45.25m, 19.83m), null, null, null);

        var community = Communities(scope).Create(accountId, request);
        return (community, new Member(accountId, MembershipId(scope, accountId, community.Id)));
    }

    protected static Member AddConfirmedMember(IServiceScope scope, Guid issuerAccountId, Guid communityId)
    {
        var member = AddUnconfirmedMember(scope, issuerAccountId, communityId);
        var certification = scope.ServiceProvider.GetRequiredService<ICertificationService>();

        var challenge = certification.CreateChallenge(issuerAccountId, communityId);
        certification.Confirm(member.AccountId, new ConfirmCertificationRequestDto(challenge.Token));

        return member;
    }

    protected static Member AddUnconfirmedMember(IServiceScope scope, Guid issuerAccountId, Guid communityId)
    {
        var accountId = NewAccount(scope);
        var qr = Communities(scope).GetQr(issuerAccountId, communityId);

        Communities(scope).Join(accountId, new JoinCommunityRequestDto(qr.QrToken));

        return new Member(accountId, MembershipId(scope, accountId, communityId));
    }

    protected static Guid NewAccount(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var name = $"user-{Guid.NewGuid():N}";
        var account = new Account(name, $"{name}@test.local", "salt.hash", DateTime.UtcNow);
        db.Accounts.Add(account);
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return account.Id;
    }

    protected static T Value<T>(IActionResult result) => (T)((ObjectResult)result).Value!;

    private static ICommunityService Communities(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICommunityService>();

    private static Guid MembershipId(IServiceScope scope, Guid accountId, Guid communityId) =>
        (scope.ServiceProvider.GetRequiredService<IMembershipService>().GetMine(accountId, communityId)).MembershipId;

    private static TController As<TController>(TController controller, Guid accountId)
        where TController : ControllerBase
    {
        controller.ControllerContext = BuildContext(accountId.ToString());
        return controller;
    }
}
