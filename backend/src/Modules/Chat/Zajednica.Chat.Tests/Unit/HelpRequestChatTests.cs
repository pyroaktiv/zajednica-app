using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Core.Domain;

namespace Zajednica.Chat.Tests.Unit;

public class HelpRequestChatTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid HelpRequest = Guid.NewGuid();
    private static readonly Guid Requester = Guid.NewGuid();
    private static readonly Guid Helper = Guid.NewGuid();

    private static HelpRequestChat Chat(bool firstResponse = true) =>
        new(Community, HelpRequest, Requester, Helper, firstResponse, Now);

    [Fact]
    public void The_requester_rewards_the_helper_within_the_allowed_range()
    {
        var chat = Chat();

        Should.Throw<ForbiddenException>(() => chat.ConcludeWithReward(Helper, 100));
        Should.Throw<EntityValidationException>(() => chat.ConcludeWithReward(Requester, HelpRequestChat.MaxReward + 1));

        var awarded = chat.ConcludeWithReward(Requester, 100);

        awarded.ShouldBe(100);
        chat.Status.ShouldBe(HelpRequestChatStatus.Concluded);
        chat.AwardedStars.ShouldBe(100);
    }

    [Fact]
    public void Goodwill_is_rewarded_only_on_a_first_response()
    {
        Chat().ConcludeWithoutReward(Requester).ShouldBe(HelpRequestChat.GoodwillReward);
        Chat(firstResponse: false).ConcludeWithoutReward(Requester).ShouldBe(0);
    }

    [Fact]
    public void Only_the_helper_resigns_and_earns_nothing()
    {
        var chat = Chat();

        Should.Throw<ForbiddenException>(() => chat.Resign(Requester));

        chat.Resign(Helper);

        chat.Status.ShouldBe(HelpRequestChatStatus.HelperResigned);
        chat.AwardedStars.ShouldBe(0);
    }

    [Fact]
    public void A_concluded_chat_takes_no_further_messages_and_is_not_concluded_twice()
    {
        var chat = Chat();
        chat.SendText(Helper, "Stizem za pola sata", true, Now);

        chat.ConcludeWithReward(Requester, HelpRequestChat.MinReward);

        chat.CanSend(Helper, true).ShouldBeFalse();
        Should.Throw<EntityValidationException>(() => chat.SendText(Helper, "Jos jedna", true, Now));
        Should.Throw<EntityValidationException>(() => chat.ConcludeWithoutReward(Requester));
        Should.Throw<EntityValidationException>(() => chat.Resign(Helper));
    }
}
