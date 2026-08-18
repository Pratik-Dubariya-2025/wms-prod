using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class UserPermissionOverrideConfiguration : IEntityTypeConfiguration<UserPermissionOverride>
    {
        public void Configure(EntityTypeBuilder<UserPermissionOverride> builder)
        {
            builder.HasKey(upo => upo.Id);

            builder.HasOne(upo => upo.User)
                .WithMany(u => u.UserPermissionOverrides)
                .HasForeignKey(upo => upo.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(upo => upo.Permission)
                .WithMany()
                .HasForeignKey(upo => upo.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(upo => upo.Reason)
                .HasMaxLength(500);
        }
    }
}
