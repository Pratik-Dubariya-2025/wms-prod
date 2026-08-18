using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class FieldRestrictionConfiguration : IEntityTypeConfiguration<FieldRestriction>
    {
        public void Configure(EntityTypeBuilder<FieldRestriction> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.FieldName)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(200);

            builder.Property(f => f.RestrictionType)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20)
                .HasDefaultValue("Hide");

            builder.Property(f => f.MaskPattern)
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.HasOne(f => f.Policy)
                .WithMany(p => p.FieldRestrictions)
                .HasForeignKey(f => f.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
