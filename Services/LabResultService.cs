using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace IT_13FinalProject.Services
{
    public record LabResultListItem(
        int LabResultId,
        Guid? ExternalId,
        string PatientName,
        string TestName,
        string Summary,
        bool IsReviewed,
        DateTime ResultDate,
        DateTime? ReviewedAt,
        bool IsAbnormal,
        string? DoctorComment
    );

    public interface ILabResultService
    {
        Task<(IReadOnlyList<LabResultListItem> Items, int TotalCount)> GetPageAsync(string? searchTerm, int page, int pageSize);
        Task<LabResult?> GetByIdAsync(int labResultId);
        Task MarkReviewedAsync(int labResultId);
        Task UpdateDoctorCommentAsync(int labResultId, string? comment);
    }

    public class DatabaseLabResultService : ILabResultService
    {
        private readonly ApplicationDbContext _cloudContext;
        private readonly LocalDbContext _localContext;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabaseLabResultService(ApplicationDbContext cloudContext, LocalDbContext localContext)
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

        private static LabResultListItem ToListItem(LabResult r)
        {
            return new LabResultListItem(
                r.LabResultId,
                r.ExternalId,
                r.PatientName,
                r.TestName,
                r.ResultSummary ?? string.Empty,
                r.IsReviewed,
                r.ResultDate,
                r.ReviewedAt,
                r.IsAbnormal,
                r.DoctorComment
            );
        }

        public async Task<(IReadOnlyList<LabResultListItem> Items, int TotalCount)> GetPageAsync(string? searchTerm, int page, int pageSize)
        {
            return await WithDbLock(async () =>
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 1, 100);
                var term = (searchTerm ?? string.Empty).Trim();

                try
                {
                    var query = _cloudContext.LabResults.AsNoTracking();

                    if (term.Length > 0)
                    {
                        query = query.Where(r =>
                            r.PatientName.Contains(term) ||
                            r.TestName.Contains(term));
                    }

                    var total = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(r => r.ResultDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return (items.Select(ToListItem).ToList(), total);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    var query = _localContext.LabResults.AsNoTracking();

                    if (term.Length > 0)
                    {
                        query = query.Where(r =>
                            r.PatientName.Contains(term) ||
                            r.TestName.Contains(term));
                    }

                    var total = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(r => r.ResultDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return (items.Select(ToListItem).ToList(), total);
                }
            });
        }

        public async Task<LabResult?> GetByIdAsync(int labResultId)
        {
            return await WithDbLock(async () =>
            {
                if (labResultId <= 0) return null;

                try
                {
                    var cloud = await _cloudContext.LabResults
                        .FirstOrDefaultAsync(r => r.LabResultId == labResultId);

                    if (cloud != null) return cloud;

                    return await _localContext.LabResults
                        .FirstOrDefaultAsync(r => r.LabResultId == labResultId);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    return await _localContext.LabResults
                        .FirstOrDefaultAsync(r => r.LabResultId == labResultId);
                }
            });
        }

        public async Task MarkReviewedAsync(int labResultId)
        {
            await WithDbLock(async () =>
            {
                Exception? cloudError = null;

                try
                {
                    var cloud = await _cloudContext.LabResults.FindAsync(labResultId);
                    if (cloud != null)
                    {
                        cloud.IsReviewed = true;
                        cloud.ReviewedAt = DateTime.Now;
                        cloud.UpdatedAt = DateTime.Now;
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    cloudError = ex;
                }

                var local = await _localContext.LabResults.FindAsync(labResultId);
                if (local != null)
                {
                    local.IsReviewed = true;
                    local.ReviewedAt = DateTime.Now;
                    local.UpdatedAt = DateTime.Now;
                    await _localContext.SaveChangesAsync();
                }
                else if (cloudError == null)
                {
                    var cloud = await _cloudContext.LabResults.AsNoTracking().FirstOrDefaultAsync(r => r.LabResultId == labResultId);
                    if (cloud != null)
                    {
                        _localContext.LabResults.Add(new LabResult
                        {
                            LabResultId = cloud.LabResultId,
                            ExternalId = cloud.ExternalId,
                            PatientId = cloud.PatientId,
                            PatientName = cloud.PatientName,
                            TestName = cloud.TestName,
                            ResultSummary = cloud.ResultSummary,
                            ResultDetails = cloud.ResultDetails,
                            IsAbnormal = cloud.IsAbnormal,
                            IsReviewed = cloud.IsReviewed,
                            ReviewedAt = cloud.ReviewedAt,
                            DoctorComment = cloud.DoctorComment,
                            ResultDate = cloud.ResultDate,
                            CreatedAt = cloud.CreatedAt,
                            UpdatedAt = cloud.UpdatedAt
                        });
                        await _localContext.SaveChangesAsync();
                    }
                }
            });
        }

        public async Task UpdateDoctorCommentAsync(int labResultId, string? comment)
        {
            await WithDbLock(async () =>
            {
                Exception? cloudError = null;

                try
                {
                    var cloud = await _cloudContext.LabResults.FindAsync(labResultId);
                    if (cloud != null)
                    {
                        cloud.DoctorComment = comment;
                        cloud.UpdatedAt = DateTime.Now;
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    cloudError = ex;
                }

                var local = await _localContext.LabResults.FindAsync(labResultId);
                if (local != null)
                {
                    local.DoctorComment = comment;
                    local.UpdatedAt = DateTime.Now;
                    await _localContext.SaveChangesAsync();
                }
                else if (cloudError == null)
                {
                    var cloud = await _cloudContext.LabResults.AsNoTracking().FirstOrDefaultAsync(r => r.LabResultId == labResultId);
                    if (cloud != null)
                    {
                        _localContext.LabResults.Add(new LabResult
                        {
                            LabResultId = cloud.LabResultId,
                            ExternalId = cloud.ExternalId,
                            PatientId = cloud.PatientId,
                            PatientName = cloud.PatientName,
                            TestName = cloud.TestName,
                            ResultSummary = cloud.ResultSummary,
                            ResultDetails = cloud.ResultDetails,
                            IsAbnormal = cloud.IsAbnormal,
                            IsReviewed = cloud.IsReviewed,
                            ReviewedAt = cloud.ReviewedAt,
                            DoctorComment = cloud.DoctorComment,
                            ResultDate = cloud.ResultDate,
                            CreatedAt = cloud.CreatedAt,
                            UpdatedAt = cloud.UpdatedAt
                        });
                        await _localContext.SaveChangesAsync();
                    }
                }
            });
        }
    }
}
