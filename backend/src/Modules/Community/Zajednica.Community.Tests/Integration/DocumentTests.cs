using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Api.Internal;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class DocumentTests : BaseCommunityIntegrationTest
{
    public DocumentTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public void Only_the_manager_posts_documents_while_every_confirmed_member_reads_them()
    {
        using var scope = Factory.Services.CreateScope();
        var managerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = CreateCommunity(scope, managerId);
        var qrToken = QrToken(scope, managerId, community.Id);
        Join(scope, memberId, qrToken);
        Certify(scope, managerId, memberId, community.Id);

        var db = Db(scope);
        var managerMembership = db.Memberships.Single(m => m.AccountId == managerId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();
        scope.ServiceProvider.GetRequiredService<IInternalIntentOutcomeService>()
            .ElectManager(managerMembership.Id);

        Should.Throw<ForbiddenException>(() =>
            Documents(scope, memberId).Add(community.Id, new AddDocumentRequest("Kucni red", "https://files.local/kucni-red.pdf")));

        var added = Value<DocumentDto>((Documents(scope, managerId)
            .Add(community.Id, new AddDocumentRequest("Kucni red", "https://files.local/kucni-red.pdf"))).Result!);
        added.PostedByMembershipId.ShouldBe(managerMembership.Id);

        var page = Value<PagedResult<DocumentDto>>(
            (Documents(scope, memberId).GetPaged(community.Id, 1, 10)).Result!);
        page.TotalCount.ShouldBe(1);
        page.Results.Single().Name.ShouldBe("Kucni red");

        Documents(scope, managerId).Remove(community.Id, added.Id);

        var afterRemoval = Value<PagedResult<DocumentDto>>(
            (Documents(scope, memberId).GetPaged(community.Id, 1, 10)).Result!);
        afterRemoval.TotalCount.ShouldBe(0);
    }

    [Fact]
    public void An_unconfirmed_member_cannot_read_the_documents()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);
        var qrToken = QrToken(scope, creatorId, community.Id);
        Join(scope, newcomerId, qrToken);

        Should.Throw<ForbiddenException>(() =>
            Documents(scope, newcomerId).GetPaged(community.Id, 1, 10));
    }
}
