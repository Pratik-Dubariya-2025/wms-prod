using Microsoft.EntityFrameworkCore;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Designation> Designations => Set<Designation>();
        public DbSet<ForgotPassword> ForgotPasswords => Set<ForgotPassword>();
        public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
        public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<AccessPolicy> AccessPolicies => Set<AccessPolicy>();
        public DbSet<PolicyCondition> PolicyConditions => Set<PolicyCondition>();
        public DbSet<FieldRestriction> FieldRestrictions => Set<FieldRestriction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Apply global conventions for all BaseModel-derived entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseModel).IsAssignableFrom(entityType.ClrType))
                {
                    // Global query filter for soft delete
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseModel.IsDeleted));
                    var filter = System.Linq.Expressions.Expression.Lambda(
                        System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                        parameter
                    );
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);

                    // Apply VARCHAR + MaxLength to BaseModel audit string fields
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseModel.CreatedBy))
                        .IsUnicode(false)
                        .HasMaxLength(100);

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseModel.ModifiedBy))
                        .IsUnicode(false)
                        .HasMaxLength(100);

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseModel.DeletedBy))
                        .IsUnicode(false)
                        .HasMaxLength(100);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseModel>();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        entry.Entity.CreatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;

                        entry.Property(nameof(BaseModel.CreatedAt)).IsModified = false;
                        entry.Property(nameof(BaseModel.CreatedBy)).IsModified = false;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
