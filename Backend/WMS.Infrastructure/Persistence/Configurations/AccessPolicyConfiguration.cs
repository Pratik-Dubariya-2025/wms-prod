using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class AccessPolicyConfiguration : IEntityTypeConfiguration<AccessPolicy>
    {
        public void Configure(EntityTypeBuilder<AccessPolicy> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Action)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.Property(p => p.Effect)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(10)
                .HasDefaultValue("Allow");

            builder.Property(p => p.RowFilterExpression)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.Priority)
                .HasDefaultValue(100);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(p => p.Module)
                .WithMany()
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Role)
                .WithMany()
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Conditions)
                .WithOne(c => c.Policy)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.FieldRestrictions)
                .WithOne(f => f.Policy)
                .HasForeignKey(f => f.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Check constraint: either RoleId or UserId must be set, not both
            builder.ToTable("AccessPolicies", t => t.HasCheckConstraint("CK_AccessPolicy_Target",
                "(\"RoleId\" IS NOT NULL AND \"UserId\" IS NULL) OR (\"RoleId\" IS NULL AND \"UserId\" IS NOT NULL)"));
        }
    }
}
