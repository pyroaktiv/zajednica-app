using Microsoft.EntityFrameworkCore;
using Zajednica.BuildingBlocks.Core.UseCases;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using ChatAggregate = Zajednica.Chat.Core.Domain.Chat;

namespace Zajednica.Chat.Infrastructure.Database.Repositories;

internal sealed class ChatEfRepository(ChatDbContext db) : IChatRepository
{
    public void Add(ChatAggregate chat)
    {
        db.Chats.Add(chat);
        db.SaveChanges();
    }

    public void Update(ChatAggregate chat) => db.SaveChanges();

    public ChatAggregate? Get(Guid id) =>
        db.Chats.Include(c => c.Participants).FirstOrDefault(c => c.Id == id);

    public CursorPage<TChat, PageCursor> GetPage<TChat>(Guid communityId, Guid membershipId, PageCursor? before, int limit)
        where TChat : ChatAggregate
    {
        var query = db.Chats
            .AsNoTracking()
            .OfType<TChat>()
            .Include(c => c.Participants)
            .Where(c => c.CommunityId == communityId && c.Participants.Any(p => p.MembershipId == membershipId));

        if (before is { } cursor)
            query = query.Where(c => c.LastActivityAt < cursor.At
                                     || (c.LastActivityAt == cursor.At && c.Id.CompareTo(cursor.Id) < 0));

        var items = query
            .OrderByDescending(c => c.LastActivityAt)
            .ThenByDescending(c => c.Id)
            .Take(limit)
            .ToList();

        return new CursorPage<TChat, PageCursor>(items,
            items.Count < limit ? null : new PageCursor(items[^1].LastActivityAt, items[^1].Id));
    }

    public DirectChat? GetDirect(Guid communityId, Guid membershipId, Guid otherMembershipId) =>
        db.Chats.OfType<DirectChat>()
            .Include(c => c.Participants)
            .FirstOrDefault(c => c.CommunityId == communityId
                                 && c.Participants.Any(p => p.MembershipId == membershipId)
                                 && c.Participants.Any(p => p.MembershipId == otherMembershipId));

    public TemporaryChat? GetTemporary(Guid communityId, Guid uncertifiedMembershipId, Guid issuerMembershipId) =>
        db.Chats.OfType<TemporaryChat>()
            .Include(c => c.Participants)
            .FirstOrDefault(c => c.CommunityId == communityId
                                 && c.Participants.Any(p => p.MembershipId == uncertifiedMembershipId)
                                 && c.Participants.Any(p => p.MembershipId == issuerMembershipId));

    public HelpRequestChat? GetActiveHelp(Guid helpRequestId, Guid helperMembershipId) =>
        db.Chats.OfType<HelpRequestChat>()
            .Include(c => c.Participants)
            .FirstOrDefault(c => c.HelpRequestId == helpRequestId
                                 && c.Status == HelpRequestChatStatus.Active
                                 && c.Participants.Any(p => p.MembershipId == helperMembershipId
                                                            && p.Role == ChatParticipantRole.Helper));

    public bool HasResponded(Guid helpRequestId, Guid helperMembershipId) =>
        db.Chats.OfType<HelpRequestChat>()
            .Any(c => c.HelpRequestId == helpRequestId
                      && c.Participants.Any(p => p.MembershipId == helperMembershipId
                                                 && p.Role == ChatParticipantRole.Helper));

    public void RemoveTemporary(Guid communityId, Guid membershipId)
    {
        var chats = db.Chats.OfType<TemporaryChat>()
            .Where(c => c.CommunityId == communityId && c.Participants.Any(p => p.MembershipId == membershipId))
            .ToList();

        if (chats.Count == 0)
            return;

        db.Chats.RemoveRange(chats);
        db.SaveChanges();
    }

    public CursorPage<Message, PageCursor> GetMessagePage(Guid chatId, PageCursor? after, int limit)
    {
        var query = db.Messages.AsNoTracking().Where(m => m.ChatId == chatId);

        if (after is { } cursor)
            query = query.Where(m => m.Date > cursor.At
                                     || (m.Date == cursor.At && m.Id.CompareTo(cursor.Id) > 0));

        var items = query
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .Take(limit)
            .ToList();

        return new CursorPage<Message, PageCursor>(items,
            items.Count < limit ? null : new PageCursor(items[^1].Date, items[^1].Id));
    }
}
