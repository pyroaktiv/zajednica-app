using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Public;

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
        scope.ServiceProvider.GetRequiredService<IInternalMembershipCommandService>()
            .ElectManager(managerMembership.Id);

        Should.Throw<ForbiddenException>(() =>
            Documents(scope, memberId).Add(community.Id, new AddDocumentRequestDto("Kucni red", "https://files.local/kucni-red.pdf")));

        var added = Value<DocumentDto>((Documents(scope, managerId)
            .Add(community.Id, new AddDocumentRequestDto("Kucni red", "https://files.local/kucni-red.pdf"))).Result!);
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

    [Fact]
    public void A_confirmed_member_resolves_the_content_key_while_an_unconfirmed_one_is_refused()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = CreateCommunity(scope, creatorId);
        var qrToken = QrToken(scope, creatorId, community.Id);
        Join(scope, newcomerId, qrToken);

        var db = Db(scope);
        var creatorMembership = db.Memberships.Single(m => m.AccountId == creatorId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();
        scope.ServiceProvider.GetRequiredService<IInternalMembershipCommandService>()
            .ElectManager(creatorMembership.Id);

        var added = Value<DocumentDto>((Documents(scope, creatorId)
            .Add(community.Id, new AddDocumentRequestDto("Kucni red", "documents/kucni-red.pdf"))).Result!);

        var documents = scope.ServiceProvider.GetRequiredService<IDocumentService>();

        var reference = documents.GetContent(creatorId, community.Id, added.Id);
        reference.Key.ShouldBe("documents/kucni-red.pdf");
        reference.DownloadName.ShouldBe("Kucni red.pdf");

        Should.Throw<ForbiddenException>(() =>
            documents.GetContent(newcomerId, community.Id, added.Id));
    }
}
