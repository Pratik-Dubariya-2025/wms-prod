using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
    {
        public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
        {
            builder.ToTable("EmployeeProfiles");

            builder.HasKey(ep => ep.Id);

            builder.HasIndex(ep => ep.UserId)
                .IsUnique();

            builder.Property(ep => ep.Salary)
                .IsRequired()
                .HasColumnType("decimal(15,2)");

            builder.Property(ep => ep.BankAccountNo)
                .HasMaxLength(500);

            builder.Property(ep => ep.BankIfsc)
                .HasMaxLength(20);

            builder.Property(ep => ep.PanNumber)
                .HasMaxLength(500);

            builder.Property(ep => ep.AadhaarLast4)
                .HasMaxLength(4)
                .IsFixedLength();

            builder.Property(ep => ep.EmergencyContactName)
                .HasMaxLength(200);

            builder.Property(ep => ep.EmergencyContactPhone)
                .HasMaxLength(30);

            builder.Property(ep => ep.BloodGroup)
                .HasMaxLength(10);

            builder.Property(ep => ep.Address)
                .HasMaxLength(1000);

            builder.HasOne(ep => ep.User)
                .WithOne()
                .HasForeignKey<EmployeeProfile>(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
