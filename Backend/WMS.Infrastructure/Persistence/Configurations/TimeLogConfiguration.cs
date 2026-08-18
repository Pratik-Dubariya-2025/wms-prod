using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
    {
        public void Configure(EntityTypeBuilder<TimeLog> builder)
        {
            builder.ToTable("TimeLogs");

            builder.HasKey(tl => tl.Id);

            builder.Property(tl => tl.LoggedHours)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(tl => tl.LogDate)
                .IsRequired();

            builder.Property(tl => tl.Notes)
                .HasMaxLength(1000);

            builder.Property(tl => tl.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(tl => tl.Task)
                .WithMany(t => t.TimeLogs)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tl => tl.User)
                .WithMany()
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(tl => tl.ApprovedBy)
                .WithMany()
                .HasForeignKey(tl => tl.ApprovedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
