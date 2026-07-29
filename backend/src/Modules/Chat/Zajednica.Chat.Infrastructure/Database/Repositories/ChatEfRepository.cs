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

    public CursorPage<ChatAggregate> GetPage(Guid communityId, Guid membershipId, DateTime? before, int limit)
    {
        var query = db.Chats
            .AsNoTracking()
            .Include(c => c.Participants)
            .Where(c => c.CommunityId == communityId && c.Participants.Any(p => p.MembershipId == membershipId));

        if (before is not null)
            query = query.Where(c => c.LastActivityAt < before);

        var items = query
            .OrderByDescending(c => c.LastActivityAt)
            .Take(limit)
            .ToList();

        return new CursorPage<ChatAggregate>(items, items.Count < limit ? null : items[^1].LastActivityAt);
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

    public CursorPage<Message> GetMessagePage(Guid chatId, DateTime? after, int limit)
    {
        var query = db.Messages.AsNoTracking().Where(m => m.ChatId == chatId);

        if (after is not null)
            query = query.Where(m => m.Date > after);

        var items = query
            .OrderBy(m => m.Date)
            .Take(limit)
            .ToList();

        return new CursorPage<Message>(items, items.Count < limit ? null : items[^1].Date);
    }
}
