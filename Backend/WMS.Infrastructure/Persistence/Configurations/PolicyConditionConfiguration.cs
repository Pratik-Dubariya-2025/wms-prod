using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class PolicyConditionConfiguration : IEntityTypeConfiguration<PolicyCondition>
    {
        public void Configure(EntityTypeBuilder<PolicyCondition> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ConditionGroup)
                .HasDefaultValue(0);

            builder.Property(c => c.Attribute)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(200);

            builder.Property(c => c.Operator)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);

            builder.Property(c => c.Value)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(c => c.Policy)
                .WithMany(p => p.Conditions)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
