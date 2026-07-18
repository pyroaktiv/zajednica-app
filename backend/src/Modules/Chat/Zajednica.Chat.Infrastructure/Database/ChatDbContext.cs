using Microsoft.EntityFrameworkCore;

namespace Zajednica.Chat.Infrastructure.Database;

public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("chat");
        base.OnModelCreating(modelBuilder);
    }
}