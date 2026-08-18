using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // Composite Primary Key
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed: ADMIN role gets ALL permissions
            var adminRoleId = Guid.Parse("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98");
            var employeeRoleId = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09");
            var hrManagerRoleId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a");
            var teamLeadRoleId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b");

            builder.HasData(
                // ADMIN gets all permissions
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },

                // EMPLOYEE gets read-only permissions
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },

                // HR_MANAGER gets all permissions
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000004") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },

                // TEAM_LEAD gets all read permissions + updates/creates
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("10000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("30000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("40000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("80000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") }
            );
        }
    }
}
