using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace IT_13FinalProject.Services
{
    public enum AuditLogSort
    {
        NewestFirst,
        OldestFirst
    }

    public record AuditLogQuery(
        string? Search,
        DateTime? FromUtc,
        DateTime? ToUtc,
        string? Role,
        AuditLogSort Sort,
        int Take
    );

    public record AuditLogStats(int Total, int Today, int ThisWeek, int ThisMonth);

    public interface IAuditLogService
    {
        Task<IReadOnlyList<AuditLog>> QueryAsync(AuditLogQuery query);
        Task<AuditLogStats> GetStatsAsync(DateTime nowLocal);
        Task WriteAsync(AuditLog log);
    }

    public class DatabaseAuditLogService : IAuditLogService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseAuditLogService(IServiceScopeFactory scopeFactory)
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

        private static async Task EnsureAuditLogsTableExistsAsync(DbContext ctx)
        {
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='audit_logs' AND xtype='U')
BEGIN
    CREATE TABLE audit_logs (
        audit_log_id INT IDENTITY(1,1) PRIMARY KEY,
        timestamp_utc DATETIME2 NOT NULL,
        user_name NVARCHAR(200) NOT NULL,
        role NVARCHAR(50) NOT NULL,
        action NVARCHAR(500) NOT NULL,
        ip_address NVARCHAR(100) NULL,
        device NVARCHAR(200) NULL,
        details NVARCHAR(MAX) NULL
    )
END";

            await ctx.Database.ExecuteSqlRawAsync(sql);
        }

        private static IQueryable<AuditLog> ApplyQuery(IQueryable<AuditLog> q, AuditLogQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                q = q.Where(x => x.UserName.Contains(s) || x.Role.Contains(s) || x.Action.Contains(s) || (x.Details != null && x.Details.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var r = query.Role.Trim();
                q = q.Where(x => x.Role == r);
            }

            if (query.FromUtc is not null)
                q = q.Where(x => x.TimestampUtc >= query.FromUtc.Value);

            if (query.ToUtc is not null)
                q = q.Where(x => x.TimestampUtc <= query.ToUtc.Value);

            q = query.Sort == AuditLogSort.OldestFirst
                ? q.OrderBy(x => x.TimestampUtc)
                : q.OrderByDescending(x => x.TimestampUtc);

            return q.Take(Math.Clamp(query.Take, 1, 500));
        }

        public async Task<IReadOnlyList<AuditLog>> QueryAsync(AuditLogQuery query)
        {
            query = query with { Take = Math.Clamp(query.Take, 1, 500) };

            using var scope = _scopeFactory.CreateScope();
            var cloudContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var localContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureAuditLogsTableExistsAsync(cloudContext);
                var q = ApplyQuery(cloudContext.AuditLogs.AsNoTracking(), query);
                return await q.ToListAsync();
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureAuditLogsTableExistsAsync(localContext);
                var q = ApplyQuery(localContext.AuditLogs.AsNoTracking(), query);
                return await q.ToListAsync();
            }
        }

        public async Task<AuditLogStats> GetStatsAsync(DateTime nowLocal)
        {
            using var scope = _scopeFactory.CreateScope();
            var cloudContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var localContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await EnsureAuditLogsTableExistsAsync(cloudContext);
                return await BuildStatsAsync(cloudContext, nowLocal);
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureAuditLogsTableExistsAsync(localContext);
                return await BuildStatsAsync(localContext, nowLocal);
            }
        }

        private static async Task<AuditLogStats> BuildStatsAsync(DbContext ctx, DateTime nowLocal)
        {
            var nowUtc = nowLocal.ToUniversalTime();
            var todayStartUtc = nowLocal.Date.ToUniversalTime();
            var weekStartLocal = nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek);
            var weekStartUtc = weekStartLocal.ToUniversalTime();
            var monthStartUtc = new DateTime(nowLocal.Year, nowLocal.Month, 1).ToUniversalTime();

            var set = ctx.Set<AuditLog>().AsNoTracking();

            var total = await set.CountAsync();
            var today = await set.CountAsync(x => x.TimestampUtc >= todayStartUtc && x.TimestampUtc <= nowUtc);
            var thisWeek = await set.CountAsync(x => x.TimestampUtc >= weekStartUtc && x.TimestampUtc <= nowUtc);
            var thisMonth = await set.CountAsync(x => x.TimestampUtc >= monthStartUtc && x.TimestampUtc <= nowUtc);

            return new AuditLogStats(total, today, thisWeek, thisMonth);
        }

        public async Task WriteAsync(AuditLog log)
        {
            if (log is null) throw new ArgumentNullException(nameof(log));

            using var scope = _scopeFactory.CreateScope();
            var cloudContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var localContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            log.TimestampUtc = log.TimestampUtc == default ? DateTime.UtcNow : log.TimestampUtc;

            try
            {
                await EnsureAuditLogsTableExistsAsync(cloudContext);
                cloudContext.AuditLogs.Add(log);
                await cloudContext.SaveChangesAsync();
            }
            catch (Exception ex) when (IsConnectionFailure(ex))
            {
                await EnsureAuditLogsTableExistsAsync(localContext);
                localContext.AuditLogs.Add(log);
                await localContext.SaveChangesAsync();
                return;
            }

            try
            {
                await EnsureAuditLogsTableExistsAsync(localContext);
                var localCopy = new AuditLog
                {
                    TimestampUtc = log.TimestampUtc,
                    UserName = log.UserName,
                    Role = log.Role,
                    Action = log.Action,
                    IpAddress = log.IpAddress,
                    Device = log.Device,
                    Details = log.Details
                };
                localContext.AuditLogs.Add(localCopy);
                await localContext.SaveChangesAsync();
            }
            catch
            {
            }
        }

        public static string BuildCsv(IEnumerable<AuditLog> logs, bool includeTime, bool includeUser, bool includeAction, bool includeDetails)
        {
            static string Esc(string? s)
            {
                s ??= string.Empty;
                if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                {
                    s = s.Replace("\"", "\"\"");
                    return $"\"{s}\"";
                }
                return s;
            }

            var headers = new List<string>();
            if (includeTime) headers.Add("Timestamp");
            if (includeUser) headers.Add("User");
            headers.Add("Role");
            if (includeAction) headers.Add("Action");
            headers.Add("IP");
            headers.Add("Device");
            if (includeDetails) headers.Add("Details");

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(Esc)));

            foreach (var l in logs)
            {
                var cols = new List<string>();
                if (includeTime) cols.Add(Esc(l.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")));
                if (includeUser) cols.Add(Esc(l.UserName));
                cols.Add(Esc(l.Role));
                if (includeAction) cols.Add(Esc(l.Action));
                cols.Add(Esc(l.IpAddress));
                cols.Add(Esc(l.Device));
                if (includeDetails) cols.Add(Esc(l.Details));
                sb.AppendLine(string.Join(",", cols));
            }

            return sb.ToString();
        }

        public static string ToBase64Utf8(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }
    }
}
