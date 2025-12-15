using Microsoft.EntityFrameworkCore;
using IT_13FinalProject.Models;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace IT_13FinalProject.Services
{
    public interface IPatientBillService
    {
        Task<IReadOnlyList<PatientBill>> GetAllAsync(bool includeArchived = false);
        Task<IReadOnlyList<PatientBill>> GetActiveAsync();
        Task<IReadOnlyList<PatientBill>> GetArchivedAsync();
        Task<PatientBill?> GetByIdAsync(int id);
        Task<PatientBill> CreateAsync(PatientBill bill);
        Task<PatientBill> UpdateAsync(PatientBill bill);
        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
        Task DeleteAsync(int id);
        Task<IReadOnlyList<PatientBill>> SearchAsync(string searchTerm);
    }

    public class DatabasePatientBillService : IPatientBillService
    {
        private readonly Data.ApplicationDbContext _cloudContext;
        private readonly Data.LocalDbContext _localContext;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabasePatientBillService(Data.ApplicationDbContext cloudContext, Data.LocalDbContext localContext)
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

        private async Task<int> GetNextLocalBillIdAsync()
        {
            var maxId = await _localContext.PatientBills.MaxAsync(b => (int?)b.BillId) ?? 0;
            return maxId + 1;
        }

        private async Task EnsureLocalBillIdAsync(PatientBill bill)
        {
            if (bill.BillId != 0)
                return;

            bill.BillId = await GetNextLocalBillIdAsync();
        }

        public async Task<IReadOnlyList<PatientBill>> GetAllAsync(bool includeArchived = false)
        {
            return await WithDbLock(async () =>
            {
                try
                {
                    return await _cloudContext.PatientBills
                        .Where(b => includeArchived || !b.IsArchived)
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in GetAllAsync. Falling back to local. {ex.GetBaseException().Message}");
                    return await _localContext.PatientBills
                        .Where(b => includeArchived || !b.IsArchived)
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
            });
        }

        public async Task<IReadOnlyList<PatientBill>> GetActiveAsync()
        {
            return await WithDbLock(async () =>
            {
                try
                {
                    return await _cloudContext.PatientBills
                        .Where(b => !b.IsArchived)
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in GetActiveAsync. Falling back to local. {ex.GetBaseException().Message}");
                    return await _localContext.PatientBills
                        .Where(b => !b.IsArchived)
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
            });
        }

        public async Task<IReadOnlyList<PatientBill>> GetArchivedAsync()
        {
            return await WithDbLock(async () =>
            {
                try
                {
                    return await _cloudContext.PatientBills
                        .Include(b => b.Patient)
                        .Where(b => b.IsArchived)
                        .OrderByDescending(b => b.ArchivedAt)
                        .ToListAsync();
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in GetArchivedAsync. Falling back to local. {ex.GetBaseException().Message}");
                    return await _localContext.PatientBills
                        .Include(b => b.Patient)
                        .Where(b => b.IsArchived)
                        .OrderByDescending(b => b.ArchivedAt)
                        .ToListAsync();
                }
            });
        }

        public async Task<PatientBill?> GetByIdAsync(int id)
        {
            return await WithDbLock(async () =>
            {
                if (id <= 0)
                    return null;

                try
                {
                    var cloudBill = await _cloudContext.PatientBills
                        .FirstOrDefaultAsync(b => b.BillId == id);

                    if (cloudBill != null)
                        return cloudBill;

                    // If cloud is reachable but the record doesn't exist there, it may exist locally.
                    return await _localContext.PatientBills
                        .FirstOrDefaultAsync(b => b.BillId == id);
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in GetByIdAsync. Falling back to local. {ex.GetBaseException().Message}");
                    return await _localContext.PatientBills
                        .FirstOrDefaultAsync(b => b.BillId == id);
                }
            });
        }

        public async Task<PatientBill> CreateAsync(PatientBill bill)
        {
            return await WithDbLock(async () =>
            {
                try
                {
                    bill.CreatedAt = DateTime.Now;
                    bill.UpdatedAt = DateTime.Now;
                    
                    Console.WriteLine($"Creating bill: {bill.PatientName}");
                    
                    // Save to cloud database first
                    _cloudContext.PatientBills.Add(bill);
                    var saveResult = await _cloudContext.SaveChangesAsync();
                    Console.WriteLine($"[BILL DEBUG] SaveChangesAsync result: {saveResult} records affected");
                    Console.WriteLine($"[BILL DEBUG] Bill saved to cloud with ID: {bill.BillId}");
                    Console.WriteLine($"[BILL DEBUG] Bill name: {bill.PatientName}");

                    // Also save to local database (follow AdminPatientManagement pattern)
                    try
                    {
                        var localBill = new PatientBill
                        {
                            BillId = bill.BillId,
                            PatientId = bill.PatientId,
                            PatientName = bill.PatientName,
                            PatientEmail = bill.PatientEmail,
                            AssessmentStatus = bill.AssessmentStatus,
                            DateOfVisit = bill.DateOfVisit,
                            TotalAmount = bill.TotalAmount,
                            InsuranceCoverage = bill.InsuranceCoverage,
                            PatientResponsibility = bill.PatientResponsibility,
                            PaymentStatus = bill.PaymentStatus,
                            PaymentMethod = bill.PaymentMethod,
                            PaymentDate = bill.PaymentDate,
                            InsuranceProvider = bill.InsuranceProvider,
                            CreatedAt = bill.CreatedAt,
                            UpdatedAt = bill.UpdatedAt,
                            IsArchived = bill.IsArchived,
                            ArchivedAt = bill.ArchivedAt
                        };
                        
                        _localContext.PatientBills.Add(localBill);
                        await _localContext.SaveChangesAsync();
                        Console.WriteLine($"[BILL DEBUG] Bill also saved to local database");
                    }
                    catch (Exception localEx)
                    {
                        Console.WriteLine($"[BILL DEBUG] Failed to save to local database: {localEx.Message}");
                        // Don't throw - cloud save was successful
                    }
                    
                    return bill;
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in CreateAsync. Saving to local only. {ex.GetBaseException().Message}");

                    bill.CreatedAt = DateTime.Now;
                    bill.UpdatedAt = DateTime.Now;

                    await EnsureLocalBillIdAsync(bill);

                    _localContext.PatientBills.Add(bill);
                    await _localContext.SaveChangesAsync();
                    return bill;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BILL DEBUG] Error creating bill: {ex.Message}");
                    Console.WriteLine($"[BILL DEBUG] Inner exception: {ex.InnerException?.Message}");
                    throw; // Re-throw to show the error in the UI
                }
            });
        }

        public async Task<PatientBill> UpdateAsync(PatientBill bill)
        {
            return await WithDbLock(async () =>
            {
                bill.UpdatedAt = DateTime.Now;

                var cloudUpdated = false;

                try
                {
                    // Update cloud database
                    _cloudContext.PatientBills.Update(bill);
                    await _cloudContext.SaveChangesAsync();
                    cloudUpdated = true;
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in UpdateAsync. Updating local only. {ex.GetBaseException().Message}");
                }

                // Also update local database (always attempt local update so app works offline)
                var localBill = await _localContext.PatientBills.FirstOrDefaultAsync(b => b.BillId == bill.BillId);
                if (localBill != null)
                {
                    localBill.PatientId = bill.PatientId;
                    localBill.PatientName = bill.PatientName;
                    localBill.PatientEmail = bill.PatientEmail;
                    localBill.AssessmentStatus = bill.AssessmentStatus;
                    localBill.DateOfVisit = bill.DateOfVisit;
                    localBill.TotalAmount = bill.TotalAmount;
                    localBill.InsuranceCoverage = bill.InsuranceCoverage;
                    localBill.PatientResponsibility = bill.PatientResponsibility;
                    localBill.PaymentStatus = bill.PaymentStatus;
                    localBill.PaymentMethod = bill.PaymentMethod;
                    localBill.PaymentDate = bill.PaymentDate;
                    localBill.InsuranceProvider = bill.InsuranceProvider;
                    localBill.UpdatedAt = bill.UpdatedAt;
                    localBill.IsArchived = bill.IsArchived;
                    localBill.ArchivedAt = bill.ArchivedAt;

                    _localContext.PatientBills.Update(localBill);
                    await _localContext.SaveChangesAsync();
                }
                else if (!cloudUpdated)
                {
                    // If it doesn't exist locally and cloud is down, insert it so the UI can keep working.
                    await EnsureLocalBillIdAsync(bill);
                    _localContext.PatientBills.Add(bill);
                    await _localContext.SaveChangesAsync();
                }
                
                return bill;
            });
        }

        public async Task ArchiveAsync(int id)
        {
            await WithDbLock(async () =>
            {
                try
                {
                    Console.WriteLine($"Archiving bill with ID: {id}");
                    
                    try
                    {
                        // Archive in cloud database
                        var cloudBill = await _cloudContext.PatientBills.FindAsync(id);
                        if (cloudBill != null)
                        {
                            cloudBill.IsArchived = true;
                            cloudBill.ArchivedAt = DateTime.Now;
                            cloudBill.UpdatedAt = DateTime.Now;
                            
                            await _cloudContext.SaveChangesAsync();
                            Console.WriteLine($"Successfully archived bill {id} in cloud database");
                        }
                        else
                        {
                            Console.WriteLine($"Bill {id} not found in cloud database");
                        }
                    }
                    catch (Exception ex) when (IsCloudConnectionFailure(ex))
                    {
                        Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in ArchiveAsync. Archiving local only. {ex.GetBaseException().Message}");
                    }
                    
                    // Also archive in local database
                    var localBill = await _localContext.PatientBills.FindAsync(id);
                    if (localBill != null)
                    {
                        localBill.IsArchived = true;
                        localBill.ArchivedAt = DateTime.Now;
                        localBill.UpdatedAt = DateTime.Now;
                        
                        await _localContext.SaveChangesAsync();
                        Console.WriteLine($"Successfully archived bill {id} in local database");
                    }
                    else
                    {
                        Console.WriteLine($"Bill {id} not found in local database");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error archiving bill {id}: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    throw; // Re-throw to show the error in the UI
                }
            });
        }

        public async Task RestoreAsync(int id)
        {
            await WithDbLock(async () =>
            {
                try
                {
                    // Restore in cloud database
                    var cloudBill = await _cloudContext.PatientBills.FindAsync(id);
                    if (cloudBill != null)
                    {
                        cloudBill.IsArchived = false;
                        cloudBill.ArchivedAt = null;
                        cloudBill.UpdatedAt = DateTime.Now;
                        
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in RestoreAsync. Restoring local only. {ex.GetBaseException().Message}");
                }
                
                // Also restore in local database
                var localBill = await _localContext.PatientBills.FindAsync(id);
                if (localBill != null)
                {
                    localBill.IsArchived = false;
                    localBill.ArchivedAt = null;
                    localBill.UpdatedAt = DateTime.Now;
                    
                    await _localContext.SaveChangesAsync();
                }
            });
        }

        public async Task DeleteAsync(int id)
        {
            await WithDbLock(async () =>
            {
                try
                {
                    // Delete from cloud database
                    var cloudBill = await _cloudContext.PatientBills.FindAsync(id);
                    if (cloudBill != null)
                    {
                        _cloudContext.PatientBills.Remove(cloudBill);
                        await _cloudContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in DeleteAsync. Deleting local only. {ex.GetBaseException().Message}");
                }
                
                // Also delete from local database
                var localBill = await _localContext.PatientBills.FindAsync(id);
                if (localBill != null)
                {
                    _localContext.PatientBills.Remove(localBill);
                    await _localContext.SaveChangesAsync();
                }
            });
        }

        public async Task<IReadOnlyList<PatientBill>> SearchAsync(string searchTerm)
        {
            return await WithDbLock(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                        return await _cloudContext.PatientBills
                            .Where(b => !b.IsArchived)
                            .OrderByDescending(b => b.CreatedAt)
                            .ToListAsync();

                    return await _cloudContext.PatientBills
                        .Where(b => !b.IsArchived && 
                                   (b.PatientName.Contains(searchTerm) || 
                                    (b.PatientEmail != null && b.PatientEmail.Contains(searchTerm))))
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    Console.WriteLine($"[BILL DEBUG] Cloud DB unavailable in SearchAsync. Falling back to local. {ex.GetBaseException().Message}");
                    if (string.IsNullOrWhiteSpace(searchTerm))
                        return await _localContext.PatientBills
                            .Where(b => !b.IsArchived)
                            .OrderByDescending(b => b.CreatedAt)
                            .ToListAsync();

                    return await _localContext.PatientBills
                        .Where(b => !b.IsArchived &&
                                   (b.PatientName.Contains(searchTerm) ||
                                    (b.PatientEmail != null && b.PatientEmail.Contains(searchTerm))))
                        .OrderByDescending(b => b.CreatedAt)
                        .ToListAsync();
                }
            });
        }
    }
}
