using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
    {
        public void Configure(EntityTypeBuilder<LeaveRequest> builder)
        {
            builder.ToTable("LeaveRequests");

            builder.HasKey(lr => lr.Id);

            builder.Property(lr => lr.LeaveType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(lr => lr.FromDate)
                .IsRequired();

            builder.Property(lr => lr.ToDate)
                .IsRequired();

            builder.Property(lr => lr.DaysCount)
                .IsRequired()
                .HasColumnType("decimal(4,1)");

            builder.Property(lr => lr.Reason)
                .HasMaxLength(1000);

            builder.Property(lr => lr.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            builder.Property(lr => lr.RejectionReason)
                .HasMaxLength(500);

            builder.HasOne(lr => lr.User)
                .WithMany()
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(lr => lr.ApprovedBy)
                .WithMany()
                .HasForeignKey(lr => lr.ApprovedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
