using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TokenHash)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(512);

            builder.HasIndex(rt => rt.TokenHash)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(rt => rt.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(rt => rt.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore computed property
            builder.Ignore(rt => rt.IsActive);

            // FK: User (configured in UserConfiguration via HasMany)
        }
    }
}
