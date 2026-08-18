using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class ForgotPasswordConfiguration : IEntityTypeConfiguration<ForgotPassword>
    {
        public void Configure(EntityTypeBuilder<ForgotPassword> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.TokenHash)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(512);

            builder.HasIndex(s => s.TokenHash)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(s => s.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship
            builder.HasOne(s => s.User)
                .WithMany(u => u.ForgotPasswords)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
