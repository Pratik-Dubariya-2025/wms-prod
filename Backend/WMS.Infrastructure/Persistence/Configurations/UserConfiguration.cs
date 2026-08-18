using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.EmployeeCode)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);

            builder.HasIndex(u => u.EmployeeCode)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(u => u.FirstName)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(u => u.Username)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(256);

            builder.Property(u => u.PhoneNumber)
                .IsUnicode(false)
                .HasMaxLength(20);

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsFirstTimeLogin)
                .HasDefaultValue(true);


            builder.Property(u => u.AccessFailedCount)
                .HasDefaultValue(0);

            builder.Property(u => u.LockoutEnd)
                .IsRequired(false);

            builder.Property(u => u.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // FK: Department
            builder.HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Designation
            builder.HasOne(u => u.Designation)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Manager (self-reference, reports-to). Restrict to avoid cascade cycles.
            builder.HasOne(u => u.Manager)
                .WithMany()
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Reporting Officer (self-reference, daily officer)
            builder.HasOne(u => u.ReportingOfficer)
                .WithMany()
                .HasForeignKey(u => u.ReportingOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Team membership
            builder.HasOne(u => u.Team)
                .WithMany()
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nav: RefreshTokens
            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default users
            builder.HasData(
                new User
                {
                    Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    EmployeeCode = "EMP-001",
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@wms.com",
                    Username = "admin",
                    PasswordHash = "$2a$11$fxpkGie8ewg0L3TFvIEOY.Zh6YHwpqlOcScYRjkZKjPGp/lFCbiZW", // Admin@123
                    PhoneNumber = "1234567890",
                    IsActive = true,
                    DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // IT
                    DesignationId = Guid.Parse("60000000-0000-0000-0000-000000000004"), // Tech Lead
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                    EmployeeCode = "EMP-002",
                    FirstName = "Pratik",
                    LastName = "Dubariya",
                    Email = "pratik@wms.com",
                    Username = "pratik",
                    PasswordHash = "$2a$11$fxpkGie8ewg0L3TFvIEOY.Zh6YHwpqlOcScYRjkZKjPGp/lFCbiZW", // Admin@123
                    PhoneNumber = "9876543210",
                    IsActive = true,
                    DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), // IT
                    DesignationId = Guid.Parse("60000000-0000-0000-0000-000000000002"), // TSE
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
