using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Threading;

namespace IT_13FinalProject.Services
{
    public record NotificationFeedItem(
        Guid Id,
        string Role,
        DateTime Timestamp,
        string Type,
        string Description,
        bool IsPriority,
        bool IsRead
    );

    public interface INotificationService
    {
        Task<IReadOnlyList<NotificationFeedItem>> GetFeedAsync(string role, int take = 50);
        Task MarkReadAsync(string role, Guid id);
        Task MarkUnreadAsync(string role, Guid id);
        Task MarkAllReadAsync(string role);
        Task<int> GetUnreadCountAsync(string role);
    }

    public class InMemoryNotificationReadState
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, byte>> _readIdsByRole = new(StringComparer.OrdinalIgnoreCase);

        public bool IsRead(string role, Guid id)
        {
            if (!_readIdsByRole.TryGetValue(role, out var set))
                return false;
            return set.ContainsKey(id);
        }

        public void MarkRead(string role, Guid id)
        {
            var set = _readIdsByRole.GetOrAdd(role, _ => new ConcurrentDictionary<Guid, byte>());
            set[id] = 1;
        }

        public void MarkUnread(string role, Guid id)
        {
            if (_readIdsByRole.TryGetValue(role, out var set))
            {
                set.TryRemove(id, out _);
            }
        }

        public void MarkAllRead(string role, IEnumerable<Guid> ids)
        {
            var set = _readIdsByRole.GetOrAdd(role, _ => new ConcurrentDictionary<Guid, byte>());
            foreach (var id in ids)
                set[id] = 1;
        }

        public int CountUnread(string role, IEnumerable<Guid> ids)
        {
            if (!_readIdsByRole.TryGetValue(role, out var set))
                return ids.Count();
            var c = 0;
            foreach (var id in ids)
            {
                if (!set.ContainsKey(id)) c++;
            }
            return c;
        }
    }

    public class DatabaseNotificationService : INotificationService
    {
        private readonly ApplicationDbContext _cloudContext;
        private readonly LocalDbContext _localContext;
        private readonly InMemoryNotificationReadState _readState;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabaseNotificationService(ApplicationDbContext cloudContext, LocalDbContext localContext, InMemoryNotificationReadState readState)
        {
            _cloudContext = cloudContext;
            _localContext = localContext;
            _readState = readState;
        }

        private async Task<T> WithDbLock<T>(Func<Task<T>> action)
        {
            await _dbLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                _dbLock.Release();
            }
        }

        private static bool IsCloudConnectionFailure(Exception ex)
        {
            if (ex is SqlException)
                return true;

            if (ex is DbUpdateException dbu && dbu.InnerException is SqlException)
                return true;

            if (ex is DbUpdateException dbu2 && dbu2.GetBaseException() is SqlException)
                return true;

            return false;
        }

        private static string NormalizeRole(string role)
        {
            var r = (role ?? string.Empty).Trim();
            if (r.Length == 0) return "doctor";
            return r.ToLowerInvariant();
        }

        public async Task<IReadOnlyList<NotificationFeedItem>> GetFeedAsync(string role, int take = 50)
        {
            role = NormalizeRole(role);
            take = Math.Clamp(take, 1, 200);

            return await WithDbLock(async () =>
            {
                try
                {
                    return await BuildFeedAsync(_cloudContext, role, take);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    return await BuildFeedAsync(_localContext, role, take);
                }
            });
        }

        private async Task<IReadOnlyList<NotificationFeedItem>> BuildFeedAsync(DbContext ctx, string role, int take)
        {
            var now = DateTime.Now;
            var items = new List<(Guid Id, DateTime Ts, string Type, string Desc, bool Priority)>();

            if (role == "doctor" || role == "nurse")
            {
                var pendingLabs = await ctx.Set<LabResult>().AsNoTracking().CountAsync(l => l.IsReviewed == false);
                if (pendingLabs > 0)
                {
                    items.Add((
                        DeterministicId(role, "pending_labs"),
                        now,
                        "Lab Result",
                        $"Pending lab results awaiting review: {pendingLabs}.",
                        true));
                }

                var soon = now.AddHours(4);
                var appts = await ctx.Set<Appointment>().AsNoTracking()
                    .Where(a => !a.IsCancelled)
                    .Where(a => a.StartDateTime >= now && a.StartDateTime <= soon)
                    .OrderBy(a => a.StartDateTime)
                    .Take(3)
                    .ToListAsync();

                foreach (var a in appts)
                {
                    items.Add((
                        DeterministicId(role, $"appt_{a.ExternalId?.ToString("D") ?? a.AppointmentId.ToString()}"),
                        a.StartDateTime,
                        "Appointment",
                        $"Upcoming appointment: {a.PatientName} at {a.StartDateTime:hh:mm tt}.",
                        false));
                }
            }

            if (role == "doctor")
            {
                var from = now.AddDays(-1);
                var newPatients = await ctx.Set<Patient>().AsNoTracking()
                    .Where(p => (p.IsArchived == false || p.IsArchived == null))
                    .Where(p => p.CreatedAt != null && p.CreatedAt >= from)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                foreach (var p in newPatients)
                {
                    var name = $"{p.FirstName} {p.LastName}".Trim();
                    items.Add((
                        DeterministicId(role, $"patient_{p.PatientId}"),
                        p.CreatedAt ?? now,
                        "Consultation",
                        $"New patient added: {name}.",
                        false));
                }
            }

            if (role == "inventory")
            {
                var low = await ctx.Set<PharmacyInventory>().AsNoTracking()
                    .Where(i => i.IsArchived == false)
                    .Where(i => i.StockQuantity > 0 && i.StockQuantity <= i.ReorderLevel)
                    .OrderBy(i => i.StockQuantity)
                    .Take(5)
                    .ToListAsync();

                foreach (var i in low)
                {
                    items.Add((
                        DeterministicId(role, $"low_{i.InventoryId}"),
                        now,
                        "Low Stock",
                        $"Low stock: {i.ItemName} — {i.StockQuantity} {i.Unit} remaining.",
                        true));
                }

                var expiring = await ctx.Set<PharmacyInventory>().AsNoTracking()
                    .Where(i => i.IsArchived == false)
                    .Where(i => i.ExpiryDate != null && i.ExpiryDate <= now.AddDays(30))
                    .OrderBy(i => i.ExpiryDate)
                    .Take(5)
                    .ToListAsync();

                foreach (var i in expiring)
                {
                    items.Add((
                        DeterministicId(role, $"exp_{i.InventoryId}"),
                        i.ExpiryDate ?? now,
                        "Expiry",
                        $"Expiring soon: {i.ItemName} (expires {i.ExpiryDate:MM/dd/yyyy}).",
                        false));
                }
            }

            if (role == "billing")
            {
                // Placeholder: billing notifications depend on billing workflows. Keep minimal.
                var unpaid = await ctx.Set<PatientBill>().AsNoTracking().CountAsync(b => b.PaymentStatus != "Paid");
                if (unpaid > 0)
                {
                    items.Add((
                        DeterministicId(role, "unpaid"),
                        now,
                        "Billing",
                        $"Unpaid bills to review: {unpaid}.",
                        true));
                }
            }

            items = items.OrderByDescending(x => x.Ts).Take(take).ToList();

            return items.Select(x => new NotificationFeedItem(
                x.Id,
                role,
                x.Ts,
                x.Type,
                x.Desc,
                x.Priority,
                _readState.IsRead(role, x.Id)
            )).ToList();
        }

        private static Guid DeterministicId(string role, string key)
        {
            var input = $"{role}:{key}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);
            return new Guid(guidBytes);
        }

        public Task MarkReadAsync(string role, Guid id)
        {
            role = NormalizeRole(role);
            _readState.MarkRead(role, id);
            return Task.CompletedTask;
        }

        public Task MarkUnreadAsync(string role, Guid id)
        {
            role = NormalizeRole(role);
            _readState.MarkUnread(role, id);
            return Task.CompletedTask;
        }

        public async Task MarkAllReadAsync(string role)
        {
            role = NormalizeRole(role);
            var feed = await GetFeedAsync(role, 200);
            _readState.MarkAllRead(role, feed.Select(x => x.Id));
        }

        public async Task<int> GetUnreadCountAsync(string role)
        {
            role = NormalizeRole(role);
            var feed = await GetFeedAsync(role, 200);
            return feed.Count(x => !x.IsRead);
        }
    }
}
