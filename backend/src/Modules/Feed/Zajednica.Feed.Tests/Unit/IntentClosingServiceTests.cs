using Moq;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.BuildingBlocks.Core.Realtime;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Core.Domain;
using Zajednica.Feed.Core.Domain.Intents;
using Zajednica.Feed.Core.Domain.Intents.Initiatives;
using Zajednica.Feed.Core.Domain.RepositoryInterfaces;
using Zajednica.Feed.Core.UseCases;
using Zajednica.Feed.Core.UseCases.Intents;
using Zajednica.Feed.Core.UseCases.Queries;
using Zajednica.Identity.Api.Internal;

namespace Zajednica.Feed.Tests.Unit;

public class IntentClosingServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Community = Guid.NewGuid();
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Target = Guid.NewGuid();

    private readonly Mock<IIntentRepository> _repository = new();
    private readonly Mock<IIntentQueryStore> _queryStore = new();
    private readonly Mock<IFeedUnitOfWork> _unitOfWork = new();
    private readonly Mock<IInternalMembershipCommandService> _commands = new();

    private IntentClosingService Service() =>
        new(_repository.Object, _queryStore.Object, _unitOfWork.Object, _commands.Object, Notifier());

    [Fact]
    public void A_vote_that_lands_while_a_due_intent_is_being_closed_makes_the_close_retry_on_fresh_state()
    {
        var accepted = AcceptedBan();
        _queryStore.Setup(r => r.GetDueIds(It.IsAny<DateTime>())).Returns([accepted.Id]);
        _repository.Setup(r => r.LoadOpenByTargetMembership(Community, Target)).Returns([]);
        _repository.SetupSequence(r => r.Load(accepted.Id))
            .Returns(Reload(accepted))
            .Returns(Reload(accepted));
        _unitOfWork.SetupSequence(u => u.Save())
            .Throws(new ConcurrencyConflictException("A vote raced the close."))
            .Pass();

        Service().CloseDue();

        _repository.Verify(r => r.Load(accepted.Id), Times.Exactly(2));
        _unitOfWork.Verify(u => u.Rollback(), Times.Once);
        _unitOfWork.Verify(u => u.Commit(), Times.Once);
        _commands.Verify(c => c.Ban(Target, accepted.Id), Times.Once);
    }

    [Fact]
    public void A_due_intent_that_a_vote_already_closed_in_the_meantime_is_left_alone_on_retry()
    {
        var accepted = AcceptedBan();
        _queryStore.Setup(r => r.GetDueIds(It.IsAny<DateTime>())).Returns([accepted.Id]);
        _repository.Setup(r => r.LoadOpenByTargetMembership(Community, Target)).Returns([]);
        _repository.SetupSequence(r => r.Load(accepted.Id))
            .Returns(Reload(accepted))
            .Returns(ClosedByVote(accepted));
        _unitOfWork.SetupSequence(u => u.Save())
            .Throws(new ConcurrencyConflictException("A vote closed it first."));

        Should.NotThrow(() => Service().CloseDue());

        _unitOfWork.Verify(u => u.Save(), Times.Once);
        _unitOfWork.Verify(u => u.Rollback(), Times.Exactly(2));
        _unitOfWork.Verify(u => u.Commit(), Times.Never);
        _commands.Verify(c => c.Ban(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    private static Intent AcceptedBan()
    {
        var initiative = new UserTargetingInitiative(UserActionKind.Ban,
            new MemberStandingContext(Target, MembershipStatus.Confirmed, MembershipRole.None),
            Community, Author, 2, "Ne postuje kucni red.");

        var intent = Intent.Open(initiative, Now);
        intent.CastVote(new VoterContext(Guid.NewGuid(), Now), true, Now);
        intent.CastVote(new VoterContext(Guid.NewGuid(), Now), true, Now);

        return intent;
    }

    private static Intent Reload(Intent intent) => Intent.Load(intent.NewEvents);

    private static Intent ClosedByVote(Intent intent)
    {
        var reloaded = Reload(intent);
        reloaded.Close(Now);

        return Intent.Load([.. intent.NewEvents, .. reloaded.NewEvents]);
    }

    private static IntentNotifier Notifier()
    {
        var directory = new Mock<IInternalMembershipDirectoryService>();
        directory.Setup(d => d.GetAccountIdsByMembershipIds(It.IsAny<IReadOnlyCollection<Guid>>())).Returns([]);

        return new IntentNotifier(
            new MemberDirectory(directory.Object, new Mock<IInternalProfileService>().Object),
            Mock.Of<INotificationSender>(),
            Mock.Of<IRealtimePusher>());
    }
}
