using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // Composite Primary Key
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed user roles
            builder.HasData(
                new UserRole { UserId = Guid.Parse("70000000-0000-0000-0000-000000000001"), RoleId = Guid.Parse("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") }, // admin -> ADMIN
                new UserRole { UserId = Guid.Parse("70000000-0000-0000-0000-000000000001"), RoleId = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") }, // admin -> EMPLOYEE
                new UserRole { UserId = Guid.Parse("70000000-0000-0000-0000-000000000002"), RoleId = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") }  // pratik -> EMPLOYEE
            );
        }
    }
}
