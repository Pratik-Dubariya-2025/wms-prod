using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class LeadConfiguration : IEntityTypeConfiguration<Lead>
    {
        public void Configure(EntityTypeBuilder<Lead> builder)
        {
            builder.ToTable("Leads");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.CompanyName)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(l => l.ContactName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.ContactEmail)
                .HasMaxLength(320);

            builder.Property(l => l.ContactPhone)
                .HasMaxLength(30);

            builder.Property(l => l.Stage)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("NewLead");

            builder.Property(l => l.EstimatedValue)
                .HasColumnType("decimal(15,2)");

            builder.Property(l => l.Region)
                .HasMaxLength(100);

            builder.Property(l => l.Source)
                .HasMaxLength(100);

            builder.HasOne(l => l.Owner)
                .WithMany()
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
