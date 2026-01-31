using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IT_13FinalProject.Services
{
    public interface IRolePermissionService
    {
        Task<Dictionary<string, bool>> GetRoleStatusAsync();
        Task SetRoleEnabledAsync(string roleKey, bool enabled);
        Task<Dictionary<string, bool>> GetPermissionsAsync(string roleKey);
        Task SavePermissionsAsync(string roleKey, Dictionary<string, bool> permissions);
    }

    public class DatabaseRolePermissionService : IRolePermissionService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseRolePermissionService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private static bool IsConnectionFailure(Exception ex)
        {
            if (ex is SqlException)
                return true;

            if (ex is DbUpdateException dbu && dbu.InnerException is SqlException)
                return true;

            if (ex is DbUpdateException dbu2 && dbu2.GetBaseException() is SqlException)
                return true;

            return false;
        }

        private static async Task EnsureTablesAsync(DbContext ctx)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='role_status' AND xtype='U')
BEGIN
    CREATE TABLE role_status (
        role_status_id INT IDENTITY(1,1) PRIMARY KEY,
        role_key NVARCHAR(50) NOT NULL,
        is_enabled BIT NOT NULL
    )
    CREATE UNIQUE INDEX IX_role_status_role_key ON role_status(role_key)
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='role_permissions' AND xtype='U')
BEGIN
    CREATE TABLE role_permissions (
        role_permission_entry_id INT IDENTITY(1,1) PRIMARY KEY,
        role_key NVARCHAR(50) NOT NULL,
        permission_key NVARCHAR(100) NOT NULL,
        is_allowed BIT NOT NULL
    )
    CREATE UNIQUE INDEX IX_role_permissions_role_perm ON role_permissions(role_key, permission_key)
END";

            await ctx.Database.ExecuteSqlRawAsync(sql);
        }

        private static Dictionary<string, bool> DefaultRoleStatus => new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = true,
            ["doctor"] = true,
            ["nurse"] = true,
            ["billing"] = true,
            ["pharmacist"] = true
        };

        private static Dictionary<string, Dictionary<string, bool>> DefaultPermissions => new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["addPatient"] = true,
                ["viewPatient"] = true,
                ["appointments"] = true,
                ["addHealthRecord"] = true,
                ["viewVitalSigns"] = true,
                ["medicationSchedule"] = true,
                ["createPrescription"] = true,
                ["labResult"] = true,
                ["viewHealthRecord"] = true,
                ["updateVitalSigns"] = true,
                ["viewPrescriptions"] = true,
                ["addMedicationNotes"] = true,
                ["generateReports"] = true,
                ["viewReports"] = true,
                ["request"] = true,
                ["insuranceClaims"] = true,
                ["viewPharmacyInventory"] = true,
                ["generateBillingInvoicing"] = true,
                ["managePharmacyInventory"] = true,
                ["viewBillingInvoicing"] = true,
                ["userProfile"] = true,
                ["auditLogs"] = true,
                ["syncManagement"] = true,
                ["settings"] = true
            },
            ["doctor"] = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["addPatient"] = true,
                ["viewPatient"] = true,
                ["appointments"] = true,
                ["addHealthRecord"] = true,
                ["viewVitalSigns"] = true,
                ["medicationSchedule"] = true,
                ["createPrescription"] = true,
                ["labResult"] = true,
                ["viewHealthRecord"] = true,
                ["updateVitalSigns"] = true,
                ["viewPrescriptions"] = true,
                ["addMedicationNotes"] = true,
                ["generateReports"] = false,
                ["viewReports"] = true,
                ["request"] = true,
                ["insuranceClaims"] = false,
                ["viewPharmacyInventory"] = true,
                ["generateBillingInvoicing"] = false,
                ["managePharmacyInventory"] = false,
                ["viewBillingInvoicing"] = false,
                ["userProfile"] = true,
                ["auditLogs"] = false,
                ["syncManagement"] = false,
                ["settings"] = false
            },
            ["nurse"] = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["addPatient"] = false,
                ["viewPatient"] = true,
                ["appointments"] = true,
                ["addHealthRecord"] = true,
                ["viewVitalSigns"] = true,
                ["medicationSchedule"] = true,
                ["createPrescription"] = false,
                ["labResult"] = true,
                ["viewHealthRecord"] = true,
                ["updateVitalSigns"] = true,
                ["viewPrescriptions"] = true,
                ["addMedicationNotes"] = true,
                ["generateReports"] = false,
                ["viewReports"] = false,
                ["request"] = true,
                ["insuranceClaims"] = false,
                ["viewPharmacyInventory"] = true,
                ["generateBillingInvoicing"] = false,
                ["managePharmacyInventory"] = false,
                ["viewBillingInvoicing"] = false,
                ["userProfile"] = true,
                ["auditLogs"] = false,
                ["syncManagement"] = false,
                ["settings"] = false
            },
            ["billing"] = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["addPatient"] = false,
                ["viewPatient"] = true,
                ["appointments"] = false,
                ["addHealthRecord"] = false,
                ["viewVitalSigns"] = false,
                ["medicationSchedule"] = false,
                ["createPrescription"] = false,
                ["labResult"] = false,
                ["viewHealthRecord"] = false,
                ["updateVitalSigns"] = false,
                ["viewPrescriptions"] = false,
                ["addMedicationNotes"] = false,
                ["generateReports"] = true,
                ["viewReports"] = true,
                ["request"] = true,
                ["insuranceClaims"] = true,
                ["viewPharmacyInventory"] = true,
                ["generateBillingInvoicing"] = true,
                ["managePharmacyInventory"] = false,
                ["viewBillingInvoicing"] = true,
                ["userProfile"] = true,
                ["auditLogs"] = false,
                ["syncManagement"] = false,
                ["settings"] = false
            },
            ["pharmacist"] = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["addPatient"] = false,
                ["viewPatient"] = true,
                ["appointments"] = false,
                ["addHealthRecord"] = false,
                ["viewVitalSigns"] = true,
                ["medicationSchedule"] = true,
                ["createPrescription"] = false,
                ["labResult"] = false,
                ["viewHealthRecord"] = true,
                ["updateVitalSigns"] = false,
                ["viewPrescriptions"] = true,
                ["addMedicationNotes"] = true,
                ["generateReports"] = false,
                ["viewReports"] = false,
                ["request"] = true,
                ["insuranceClaims"] = false,
                ["viewPharmacyInventory"] = true,
                ["generateBillingInvoicing"] = false,
                ["managePharmacyInventory"] = true,
                ["viewBillingInvoicing"] = false,
                ["userProfile"] = true,
                ["auditLogs"] = false,
                ["syncManagement"] = false,
                ["settings"] = false
            }
        };

        private static async Task EnsureSeededAsync(DbContext ctx)
        {
            var statusSet = ctx.Set<RoleStatus>();
            if (!await statusSet.AsNoTracking().AnyAsync())
            {
                var statuses = DefaultRoleStatus.Select(kv => new RoleStatus { RoleKey = kv.Key.ToLowerInvariant(), IsEnabled = kv.Value }).ToList();
                statusSet.AddRange(statuses);
            }

            var permSet = ctx.Set<RolePermissionEntry>();
            if (!await permSet.AsNoTracking().AnyAsync())
            {
                var list = new List<RolePermissionEntry>();
                foreach (var (roleKey, perms) in DefaultPermissions)
                {
                    foreach (var (permKey, allowed) in perms)
                    {
                        list.Add(new RolePermissionEntry { RoleKey = roleKey.ToLowerInvariant(), PermissionKey = permKey, IsAllowed = allowed });
                    }
                }
                permSet.AddRange(list);
            }

            if (ctx.ChangeTracker.HasChanges())
                await ctx.SaveChangesAsync();
        }

        public async Task<Dictionary<string, bool>> GetRoleStatusAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var cloud = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var local = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureTablesAsync(cloud);
                await EnsureSeededAsync(cloud);
                var rows = await cloud.Set<RoleStatus>().AsNoTracking().ToListAsync();
                return rows.ToDictionary(x => x.RoleKey, x => x.IsEnabled, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                var rows = await local.Set<RoleStatus>().AsNoTracking().ToListAsync();
                return rows.ToDictionary(x => x.RoleKey, x => x.IsEnabled, StringComparer.OrdinalIgnoreCase);
            }
        }

        public async Task SetRoleEnabledAsync(string roleKey, bool enabled)
        {
            roleKey = (roleKey ?? string.Empty).Trim().ToLowerInvariant();
            if (roleKey.Length == 0) return;
            if (roleKey == "admin") return;

            using var scope = _scopeFactory.CreateScope();
            var cloud = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var local = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureTablesAsync(cloud);
                await EnsureSeededAsync(cloud);
                var row = await cloud.Set<RoleStatus>().FirstOrDefaultAsync(x => x.RoleKey == roleKey);
                if (row == null)
                {
                    cloud.Set<RoleStatus>().Add(new RoleStatus { RoleKey = roleKey, IsEnabled = enabled });
                }
                else
                {
                    row.IsEnabled = enabled;
                }
                await cloud.SaveChangesAsync();
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                var row = await local.Set<RoleStatus>().FirstOrDefaultAsync(x => x.RoleKey == roleKey);
                if (row == null)
                {
                    local.Set<RoleStatus>().Add(new RoleStatus { RoleKey = roleKey, IsEnabled = enabled });
                }
                else
                {
                    row.IsEnabled = enabled;
                }
                await local.SaveChangesAsync();
                return;
            }

            try
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                var row = await local.Set<RoleStatus>().FirstOrDefaultAsync(x => x.RoleKey == roleKey);
                if (row == null)
                {
                    local.Set<RoleStatus>().Add(new RoleStatus { RoleKey = roleKey, IsEnabled = enabled });
                }
                else
                {
                    row.IsEnabled = enabled;
                }
                await local.SaveChangesAsync();
            }
            catch
            {
            }
        }

        public async Task<Dictionary<string, bool>> GetPermissionsAsync(string roleKey)
        {
            roleKey = (roleKey ?? string.Empty).Trim().ToLowerInvariant();
            if (roleKey.Length == 0) roleKey = "admin";

            using var scope = _scopeFactory.CreateScope();
            var cloud = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var local = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureTablesAsync(cloud);
                await EnsureSeededAsync(cloud);
                var rows = await cloud.Set<RolePermissionEntry>().AsNoTracking().Where(x => x.RoleKey == roleKey).ToListAsync();
                return rows.ToDictionary(x => x.PermissionKey, x => x.IsAllowed, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                var rows = await local.Set<RolePermissionEntry>().AsNoTracking().Where(x => x.RoleKey == roleKey).ToListAsync();
                return rows.ToDictionary(x => x.PermissionKey, x => x.IsAllowed, StringComparer.OrdinalIgnoreCase);
            }
        }

        public async Task SavePermissionsAsync(string roleKey, Dictionary<string, bool> permissions)
        {
            roleKey = (roleKey ?? string.Empty).Trim().ToLowerInvariant();
            if (roleKey.Length == 0) roleKey = "admin";

            using var scope = _scopeFactory.CreateScope();
            var cloud = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var local = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureTablesAsync(cloud);
                await EnsureSeededAsync(cloud);
                await SavePermissionsToContextAsync(cloud, roleKey, permissions);
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                await SavePermissionsToContextAsync(local, roleKey, permissions);
                return;
            }

            try
            {
                await EnsureTablesAsync(local);
                await EnsureSeededAsync(local);
                await SavePermissionsToContextAsync(local, roleKey, permissions);
            }
            catch
            {
            }
        }

        private static async Task SavePermissionsToContextAsync(DbContext ctx, string roleKey, Dictionary<string, bool> permissions)
        {
            var set = ctx.Set<RolePermissionEntry>();
            var existing = await set.Where(x => x.RoleKey == roleKey).ToListAsync();

            foreach (var e in existing)
            {
                if (permissions.TryGetValue(e.PermissionKey, out var v))
                    e.IsAllowed = v;
            }

            foreach (var (permKey, allowed) in permissions)
            {
                if (existing.All(x => !string.Equals(x.PermissionKey, permKey, StringComparison.OrdinalIgnoreCase)))
                {
                    set.Add(new RolePermissionEntry { RoleKey = roleKey, PermissionKey = permKey, IsAllowed = allowed });
                }
            }

            await ctx.SaveChangesAsync();
        }
    }
}
