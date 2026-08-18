using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            builder.Property(i => i.Amount)
                .HasColumnType("decimal(15,2)");

            builder.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Draft");

            builder.Property(i => i.IssuedDate)
                .IsRequired();

            builder.Property(i => i.DueDate)
                .IsRequired();

            builder.HasOne(i => i.Lead)
                .WithMany()
                .HasForeignKey(i => i.LeadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
