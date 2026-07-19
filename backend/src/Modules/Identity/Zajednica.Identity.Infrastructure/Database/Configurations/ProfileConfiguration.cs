using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Infrastructure.Database.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");
        builder.HasKey(p => p.Id);
        
        builder.HasIndex("AccountId").IsUnique();
    }
}
