using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Api.Controllers.Community;
using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Community.Tests;

public class BaseCommunityIntegrationTest : BaseWebIntegrationTest<CommunityTestFactory>
{
    public BaseCommunityIntegrationTest(CommunityTestFactory factory) : base(factory) { }

    protected static CommunityDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<CommunityDbContext>();

    protected static CommunityController Communities(IServiceScope scope, Guid accountId) =>
        As(new CommunityController(scope.ServiceProvider.GetRequiredService<ICommunityService>()), accountId);

    protected static MembershipController Members(IServiceScope scope, Guid accountId) =>
        As(new MembershipController(scope.ServiceProvider.GetRequiredService<IMembershipService>()), accountId);

    protected static CertificationController Certification(IServiceScope scope, Guid accountId) =>
        As(new CertificationController(scope.ServiceProvider.GetRequiredService<ICertificationService>()), accountId);

    protected static DocumentController Documents(IServiceScope scope, Guid accountId) =>
        As(new DocumentController(
            scope.ServiceProvider.GetRequiredService<IDocumentService>(),
            scope.ServiceProvider.GetRequiredService<IFileStorage>()), accountId);

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

    protected static CommunityDetailsDto CreateCommunity(IServiceScope scope, Guid accountId)
    {
        var request = new CreateCommunityRequestDto(
            $"Zgrada {Guid.NewGuid():N}", new AddressDto("Bulevar", "12", 45.25m, 19.83m), null, null, null);
        return Value<CommunityDetailsDto>((Communities(scope, accountId).Create(request)).Result!);
    }

    protected static JoinedCommunityDto Join(IServiceScope scope, Guid accountId, string qrToken) =>
        Value<JoinedCommunityDto>((Communities(scope, accountId).Join(new JoinCommunityRequestDto(qrToken))).Result!);

    protected static string QrToken(IServiceScope scope, Guid ownerAccountId, Guid communityId) =>
        Value<CommunityQrDto>((Communities(scope, ownerAccountId).GetQr(communityId)).Result!).QrToken;

    protected static CertificationResultDto Certify(
        IServiceScope scope, Guid issuerAccountId, Guid candidateAccountId, Guid communityId)
    {
        var challenge = Value<CertificationChallengeDto>(
            (Certification(scope, issuerAccountId).CreateChallenge(communityId)).Result!);
        var result = Certification(scope, candidateAccountId)
            .Confirm(new ConfirmCertificationRequestDto(challenge.Token));
        return Value<CertificationResultDto>(result.Result!);
    }

    protected static T Value<T>(IActionResult result) => (T)((ObjectResult)result).Value!;

    private static TController As<TController>(TController controller, Guid accountId)
        where TController : ControllerBase
    {
        controller.ControllerContext = BuildContext(accountId.ToString());
        return controller;
    }
}
