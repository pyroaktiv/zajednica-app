using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Community.Api.Dto;
using Zajednica.Community.Api.Internal;

namespace Zajednica.Community.Tests.Integration;

[Collection("Sequential")]
public class DocumentTests : BaseCommunityIntegrationTest
{
    public DocumentTests(CommunityTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_the_manager_posts_documents_while_every_confirmed_member_reads_them()
    {
        using var scope = Factory.Services.CreateScope();
        var managerId = NewAccount(scope);
        var memberId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, managerId);
        var qrToken = await QrTokenAsync(scope, managerId, community.Id);
        await JoinAsync(scope, memberId, qrToken);
        await CertifyAsync(scope, managerId, memberId, community.Id);

        var db = Db(scope);
        var managerMembership = db.Memberships.Single(m => m.AccountId == managerId && m.CommunityId == community.Id);
        db.ChangeTracker.Clear();
        await scope.ServiceProvider.GetRequiredService<IInternalMembershipService>()
            .ElectManagerAsync(managerMembership.Id);

        await Should.ThrowAsync<ForbiddenException>(() =>
            Documents(scope, memberId).Add(community.Id, new AddDocumentRequest("Kucni red", "https://files.local/kucni-red.pdf"), default));

        var added = Value<DocumentDto>((await Documents(scope, managerId)
            .Add(community.Id, new AddDocumentRequest("Kucni red", "https://files.local/kucni-red.pdf"), default)).Result!);
        added.PostedByMembershipId.ShouldBe(managerMembership.Id);

        var page = Value<PagedResult<DocumentDto>>(
            (await Documents(scope, memberId).GetPaged(community.Id, 1, 10, default)).Result!);
        page.TotalCount.ShouldBe(1);
        page.Results.Single().Name.ShouldBe("Kucni red");

        await Documents(scope, managerId).Remove(community.Id, added.Id, default);

        var afterRemoval = Value<PagedResult<DocumentDto>>(
            (await Documents(scope, memberId).GetPaged(community.Id, 1, 10, default)).Result!);
        afterRemoval.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task An_unconfirmed_member_cannot_read_the_documents()
    {
        using var scope = Factory.Services.CreateScope();
        var creatorId = NewAccount(scope);
        var newcomerId = NewAccount(scope);
        var community = await CreateCommunityAsync(scope, creatorId);
        var qrToken = await QrTokenAsync(scope, creatorId, community.Id);
        await JoinAsync(scope, newcomerId, qrToken);

        await Should.ThrowAsync<ForbiddenException>(() =>
            Documents(scope, newcomerId).GetPaged(community.Id, 1, 10, default));
    }
}
