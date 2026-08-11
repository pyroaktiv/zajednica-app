using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.HelpChats;

namespace Zajednica.Chat.Tests.Integration;

[Collection("Sequential")]
public class HelpRequestChatTests : BaseChatIntegrationTest
{
    public HelpRequestChatTests(ChatTestFactory factory) : base(factory) { }

    [Fact]
    public void A_helper_responds_once_and_the_reward_lands_on_their_membership()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, requester) = CreateCommunity(scope);
        var helper = AddConfirmedMember(scope, requester.AccountId, community.Id);
        var post = CreateHelpRequest(scope, requester.AccountId, community.Id, "Treba mi pomoc oko selidbe.");

        var chat = Respond(scope, helper.AccountId, community.Id, post.Id);

        chat.Type.ShouldBe("HELP_REQUEST");
        ParticipantOf(chat, helper.MembershipId).Role.ShouldBe("Helper");
        chat.HelpRequestId.ShouldBe(post.Id);
        chat.Status.ShouldBe("Active");
        Respond(scope, helper.AccountId, community.Id, post.Id).Id.ShouldBe(chat.Id);

        SendText(scope, helper.AccountId, community.Id, chat.Id, "Stizem za pola sata.");
        var starsBefore = StarsOf(scope, helper.MembershipId);

        var concluded = Value<ChatDetailsDto>(HelpChats(scope, requester.AccountId)
            .ConcludeWithReward(community.Id, chat.Id, new ConcludeWithRewardRequestDto(150)).Result!);

        concluded.Status.ShouldBe("Concluded");
        concluded.AwardedStars.ShouldBe(150);
        concluded.CanSend.ShouldBeFalse();

        CommunityDb(scope).ChangeTracker.Clear();
        StarsOf(scope, helper.MembershipId).ShouldBe(starsBefore + 150);
    }

    [Fact]
    public void Neither_the_author_nor_a_late_neighbour_responds_to_a_closed_request()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, requester) = CreateCommunity(scope);
        var helper = AddConfirmedMember(scope, requester.AccountId, community.Id);
        var post = CreateHelpRequest(scope, requester.AccountId, community.Id, "Treba mi pomoc.");

        Should.Throw<EntityValidationException>(() => Respond(scope, requester.AccountId, community.Id, post.Id));

        CloseHelpRequest(scope, requester.AccountId, community.Id, post.Id);

        Should.Throw<EntityValidationException>(() => Respond(scope, helper.AccountId, community.Id, post.Id));
    }

    [Fact]
    public void A_helper_who_resigned_and_came_back_no_longer_earns_the_goodwill_stars()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, requester) = CreateCommunity(scope);
        var helper = AddConfirmedMember(scope, requester.AccountId, community.Id);
        var post = CreateHelpRequest(scope, requester.AccountId, community.Id, "Treba mi pomoc.");

        var first = Respond(scope, helper.AccountId, community.Id, post.Id);
        HelpChats(scope, helper.AccountId).Resign(community.Id, first.Id);

        var second = Respond(scope, helper.AccountId, community.Id, post.Id);
        second.Id.ShouldNotBe(first.Id);

        var starsBefore = StarsOf(scope, helper.MembershipId);
        HelpChats(scope, requester.AccountId).ConcludeWithoutReward(community.Id, second.Id);

        CommunityDb(scope).ChangeTracker.Clear();
        StarsOf(scope, helper.MembershipId).ShouldBe(starsBefore);
    }

    [Fact]
    public void Goodwill_on_a_first_response_still_thanks_the_helper_with_stars()
    {
        using var scope = Factory.Services.CreateScope();
        var (community, requester) = CreateCommunity(scope);
        var helper = AddConfirmedMember(scope, requester.AccountId, community.Id);
        var post = CreateHelpRequest(scope, requester.AccountId, community.Id, "Treba mi pomoc.");

        var chat = Respond(scope, helper.AccountId, community.Id, post.Id);
        var starsBefore = StarsOf(scope, helper.MembershipId);

        var concluded = Value<ChatDetailsDto>(HelpChats(scope, requester.AccountId)
            .ConcludeWithoutReward(community.Id, chat.Id).Result!);

        concluded.AwardedStars.ShouldBe(10);
        CommunityDb(scope).ChangeTracker.Clear();
        StarsOf(scope, helper.MembershipId).ShouldBe(starsBefore + 10);
    }
}
