using IT_13FinalProject.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading;

using DbHealthRecord = IT_13FinalProject.Models.HealthRecord;

namespace IT_13FinalProject.Services
{
    public record PrescriptionListItem(
        int RecordId,
        string Patient,
        string Doctor,
        string Items,
        string StatusKey,
        string StatusLabel,
        string Time,
        bool IsArchived
    );

    public interface IPrescriptionService
    {
        Task<(IReadOnlyList<PrescriptionListItem> Items, int TotalCount)> GetPageAsync(bool showArchived, string? searchTerm, int page, int pageSize);
        Task ArchiveAsync(int recordId);
        Task RestoreAsync(int recordId);
        Task<DbHealthRecord?> GetByRecordIdAsync(int recordId);
    }

    public class DatabasePrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _cloudContext;
        private readonly LocalDbContext _localContext;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabasePrescriptionService(ApplicationDbContext cloudContext, LocalDbContext localContext)
        {
            _cloudContext = cloudContext;
            _localContext = localContext;
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

        private async Task WithDbLock(Func<Task> action)
        {
            await _dbLock.WaitAsync();
            try
            {
                await action();
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

        private static string ToStatusKey(string? visitStatus)
        {
            var s = (visitStatus ?? string.Empty).Trim();
            if (s.Length == 0) return "pending";

            var lower = s.ToLowerInvariant();
            if (lower.Contains("active") || lower.Contains("approved") || lower.Contains("completed") || lower.Contains("done"))
                return "active";
            if (lower.Contains("pending"))
                return "pending";
            if (lower.Contains("cancel") || lower.Contains("reject"))
                return "cancelled";

            return "pending";
        }

        private static PrescriptionListItem ToListItem(DbHealthRecord r)
        {
            var patientName = r.Patient != null
                ? $"{r.Patient.FirstName} {r.Patient.LastName}".Trim()
                : (r.PatientId.HasValue ? $"Patient #{r.PatientId.Value}" : "Unknown");

            var doctorName = string.IsNullOrWhiteSpace(r.DoctorSignature) ? "" : r.DoctorSignature;
            var items = BuildItems(r);
            var statusLabel = string.IsNullOrWhiteSpace(r.VisitStatus) ? "Pending" : r.VisitStatus;
            var statusKey = ToStatusKey(r.VisitStatus);

            var dt = r.RecordDate ?? r.ApprovalDate ?? DateTime.Now;
            var time = dt.ToString("h:mm tt");

            return new PrescriptionListItem(
                r.RecordId,
                patientName,
                doctorName,
                items,
                statusKey,
                statusLabel,
                time,
                r.IsArchived
            );
        }

        private static bool HasAnyPrescriptionFields(DbHealthRecord r)
        {
            if (!string.IsNullOrWhiteSpace(r.Prescription))
                return true;

            if (!string.IsNullOrWhiteSpace(r.MedicineName))
                return true;

            if (!string.IsNullOrWhiteSpace(r.Dosage))
                return true;

            if (!string.IsNullOrWhiteSpace(r.Frequency))
                return true;

            if (!string.IsNullOrWhiteSpace(r.Duration))
                return true;

            return false;
        }

        private static string BuildItems(DbHealthRecord r)
        {
            if (!string.IsNullOrWhiteSpace(r.Prescription))
                return r.Prescription;

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(r.MedicineName)) parts.Add(r.MedicineName.Trim());
            if (!string.IsNullOrWhiteSpace(r.Dosage)) parts.Add($"{r.Dosage.Trim()}");
            if (!string.IsNullOrWhiteSpace(r.Frequency)) parts.Add($"{r.Frequency.Trim()}");
            if (!string.IsNullOrWhiteSpace(r.Duration)) parts.Add($"{r.Duration.Trim()}");

            return string.Join(" ", parts).Trim();
        }

        public async Task<(IReadOnlyList<PrescriptionListItem> Items, int TotalCount)> GetPageAsync(bool showArchived, string? searchTerm, int page, int pageSize)
        {
            return await WithDbLock(async () =>
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);
                var term = (searchTerm ?? string.Empty).Trim();

                try
                {
                    var query = _cloudContext.HealthRecords
                        .Include(r => r.Patient)
                        .Where(r => r.IsArchived == showArchived)
                        .Where(r =>
                            (r.Prescription != null && EF.Functions.DataLength(r.Prescription) > 0)
                            || (r.MedicineName != null && EF.Functions.DataLength(r.MedicineName) > 0)
                            || (r.Dosage != null && EF.Functions.DataLength(r.Dosage) > 0)
                            || (r.Frequency != null && EF.Functions.DataLength(r.Frequency) > 0)
                            || (r.Duration != null && EF.Functions.DataLength(r.Duration) > 0));

                    if (term.Length > 0)
                    {
                        query = query.Where(r =>
                            (r.Patient != null && ((r.Patient.FirstName + " " + r.Patient.LastName).Contains(term))) ||
                            (r.DoctorSignature != null && r.DoctorSignature.Contains(term)));
                    }

                    var total = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(r => r.RecordDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return (items.Select(ToListItem).ToList(), total);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    var query = _localContext.HealthRecords
                        .Include(r => r.Patient)
                        .Where(r => r.IsArchived == showArchived)
                        .Where(r =>
                            (r.Prescription != null && EF.Functions.DataLength(r.Prescription) > 0)
                            || (r.MedicineName != null && EF.Functions.DataLength(r.MedicineName) > 0)
                            || (r.Dosage != null && EF.Functions.DataLength(r.Dosage) > 0)
                            || (r.Frequency != null && EF.Functions.DataLength(r.Frequency) > 0)
                            || (r.Duration != null && EF.Functions.DataLength(r.Duration) > 0));

                    if (term.Length > 0)
                    {
                        query = query.Where(r =>
                            (r.Patient != null && ((r.Patient.FirstName + " " + r.Patient.LastName).Contains(term))) ||
                            (r.DoctorSignature != null && r.DoctorSignature.Contains(term)));
                    }

                    var total = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(r => r.RecordDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return (items.Select(ToListItem).ToList(), total);
                }
            });
        }

        public async Task<DbHealthRecord?> GetByRecordIdAsync(int recordId)
        {
            return await WithDbLock(async () =>
            {
                if (recordId <= 0) return null;

                try
                {
                    var cloud = await _cloudContext.HealthRecords
                        .Include(r => r.Patient)
                        .FirstOrDefaultAsync(r => r.RecordId == recordId);

                    if (cloud != null) return cloud;

                    return await _localContext.HealthRecords
                        .Include(r => r.Patient)
                        .FirstOrDefaultAsync(r => r.RecordId == recordId);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    return await _localContext.HealthRecords
                        .Include(r => r.Patient)
                        .FirstOrDefaultAsync(r => r.RecordId == recordId);
                }
            });
        }

        public async Task ArchiveAsync(int recordId)
        {
            await WithDbLock(async () =>
            {
                try
                {
                    var cloud = await _cloudContext.HealthRecords.FindAsync(recordId);
                    if (cloud != null)
                    {
                        cloud.IsArchived = true;
                        cloud.ArchivedAt = DateTime.Now;
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                }

                var local = await _localContext.HealthRecords.FindAsync(recordId);
                if (local != null)
                {
                    local.IsArchived = true;
                    local.ArchivedAt = DateTime.Now;
                    await _localContext.SaveChangesAsync();
                }
            });
        }

        public async Task RestoreAsync(int recordId)
        {
            await WithDbLock(async () =>
            {
                try
                {
                    var cloud = await _cloudContext.HealthRecords.FindAsync(recordId);
                    if (cloud != null)
                    {
                        cloud.IsArchived = false;
                        cloud.ArchivedAt = null;
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                }

                var local = await _localContext.HealthRecords.FindAsync(recordId);
                if (local != null)
                {
                    local.IsArchived = false;
                    local.ArchivedAt = null;
                    await _localContext.SaveChangesAsync();
                }
            });
        }
    }
}
