using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Api.Controllers.Chat;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.Messages;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Infrastructure.Database;
using Zajednica.Community.Api.Dto.Certification;
using Zajednica.Community.Api.Dto.Communities;
using Zajednica.Community.Api.Public;
using Zajednica.Community.Infrastructure.Database;
using Zajednica.Feed.Api.Dto.Posts;
using Zajednica.Feed.Api.Public;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Chat.Tests;

public class BaseChatIntegrationTest : BaseWebIntegrationTest<ChatTestFactory>
{
    public BaseChatIntegrationTest(ChatTestFactory factory) : base(factory) { }

    protected sealed record Member(Guid AccountId, Guid MembershipId);

    protected static ChatDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ChatDbContext>();

    protected static CommunityDbContext CommunityDb(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<CommunityDbContext>();

    protected static ChatController Chats(IServiceScope scope, Guid accountId) =>
        As(new ChatController(scope.ServiceProvider.GetRequiredService<IChatService>()), accountId);

    protected static MessageController Messages(IServiceScope scope, Guid accountId) =>
        As(new MessageController(scope.ServiceProvider.GetRequiredService<IMessageService>()), accountId);

    protected static HelpRequestChatController HelpChats(IServiceScope scope, Guid accountId) =>
        As(new HelpRequestChatController(
            scope.ServiceProvider.GetRequiredService<IHelpRequestChatService>()), accountId);

    protected static (CommunityDetailsDto Community, Member Owner) CreateCommunity(IServiceScope scope)
    {
        var accountId = NewAccount(scope);
        var request = new CreateCommunityRequest(
            $"Zgrada {Guid.NewGuid():N}", new AddressDto("Bulevar", "12", 45.25m, 19.83m), null, null, null);

        var community = Communities(scope).Create(accountId, request);
        return (community, new Member(accountId, MembershipId(scope, accountId, community.Id)));
    }

    protected static Member AddConfirmedMember(IServiceScope scope, Guid issuerAccountId, Guid communityId)
    {
        var member = AddUnconfirmedMember(scope, issuerAccountId, communityId);
        Confirm(scope, issuerAccountId, member.AccountId, communityId);

        return member;
    }

    protected static Member AddUnconfirmedMember(IServiceScope scope, Guid issuerAccountId, Guid communityId)
    {
        var accountId = NewAccount(scope);
        var qr = Communities(scope).GetQr(issuerAccountId, communityId);

        Communities(scope).Join(accountId, new JoinCommunityRequest(qr.QrToken));

        return new Member(accountId, MembershipId(scope, accountId, communityId));
    }

    protected static void Confirm(IServiceScope scope, Guid issuerAccountId, Guid candidateAccountId, Guid communityId)
    {
        var certification = scope.ServiceProvider.GetRequiredService<ICertificationService>();

        var challenge = certification.CreateChallenge(issuerAccountId, communityId);
        certification.Confirm(candidateAccountId, new ConfirmCertificationRequest(challenge.Token));
    }

    protected static ChatDetailsDto OpenDirect(IServiceScope scope, Guid accountId, Guid communityId,
        Guid targetMembershipId) =>
        Value<ChatDetailsDto>(Chats(scope, accountId)
            .OpenDirect(communityId, new OpenDirectChatRequest(targetMembershipId)).Result!);

    protected static ChatDetailsDto OpenTemporary(IServiceScope scope, Guid accountId, Guid communityId,
        Guid issuerMembershipId) =>
        Value<ChatDetailsDto>(Chats(scope, accountId)
            .OpenTemporary(communityId, new OpenTemporaryChatRequest(issuerMembershipId)).Result!);

    protected static MessageDto SendText(IServiceScope scope, Guid accountId, Guid communityId, Guid chatId,
        string text) =>
        Value<MessageDto>(Messages(scope, accountId).SendText(communityId, chatId, new SendTextRequest(text)).Result!);

    protected static ChatDetailsDto Respond(IServiceScope scope, Guid accountId, Guid communityId,
        Guid helpRequestId) =>
        Value<ChatDetailsDto>(HelpChats(scope, accountId).Respond(communityId, helpRequestId).Result!);

    protected static PostDto CreateHelpRequest(IServiceScope scope, Guid accountId, Guid communityId, string text) =>
        Posts(scope).CreateHelpRequest(accountId, communityId, new CreateHelpRequestRequest(text, null));

    protected static void CloseHelpRequest(IServiceScope scope, Guid accountId, Guid communityId, Guid postId) =>
        Posts(scope).CloseHelpRequest(accountId, communityId, postId);

    protected static int StarsOf(IServiceScope scope, Guid membershipId) =>
        CommunityDb(scope).Memberships.Single(m => m.Id == membershipId).Stars;

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

    protected static ChatParticipantDto ParticipantOf(ChatDetailsDto chat, Guid membershipId) =>
        chat.Participants.Single(p => p.MembershipId == membershipId);

    protected static T Value<T>(IActionResult result) => (T)((ObjectResult)result).Value!;

    private static ICommunityService Communities(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICommunityService>();

    private static IPostService Posts(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IPostService>();

    private static Guid MembershipId(IServiceScope scope, Guid accountId, Guid communityId) =>
        scope.ServiceProvider.GetRequiredService<IMembershipService>().GetMine(accountId, communityId).MembershipId;

    private static TController As<TController>(TController controller, Guid accountId)
        where TController : ControllerBase
    {
        controller.ControllerContext = BuildContext(accountId.ToString());
        return controller;
    }
}
