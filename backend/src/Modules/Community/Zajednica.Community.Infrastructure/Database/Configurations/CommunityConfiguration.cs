using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zajednica.Community.Infrastructure.Database.Configurations;

public class CommunityConfiguration : IEntityTypeConfiguration<Core.Domain.Community>
{
    public void Configure(EntityTypeBuilder<Core.Domain.Community> builder)
    {
        builder.ToTable("Communities");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired();
        builder.Property(c => c.QrToken).IsRequired();
        builder.HasIndex(c => c.QrToken).IsUnique();

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.StreetName).HasColumnName("StreetName").IsRequired();
            address.Property(a => a.StreetNumber).HasColumnName("StreetNumber").IsRequired();

            address.OwnsOne(a => a.Coordinates, coordinates =>
            {
                coordinates.Property(x => x.Latitude).HasColumnName("Latitude").HasPrecision(9, 6);
                coordinates.Property(x => x.Longitude).HasColumnName("Longitude").HasPrecision(9, 6);
            });
        });
        builder.Navigation(c => c.Address).IsRequired();

        builder.OwnsOne(c => c.RegistrationNumber, mb =>
            mb.Property(x => x.Value).HasColumnName("RegistrationNumber").HasMaxLength(8));

        builder.OwnsOne(c => c.TaxId, pib =>
            pib.Property(x => x.Value).HasColumnName("TaxId").HasMaxLength(9));
    }
}
