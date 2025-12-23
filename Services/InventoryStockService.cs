using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace IT_13FinalProject.Services
{
    public record InventoryMedicineOption(
        int InventoryId,
        string ItemName,
        int StockQuantity,
        string Unit,
        DateTime? ExpiryDate
    );

    public interface IInventoryStockService
    {
        Task<IReadOnlyList<InventoryMedicineOption>> GetAvailableMedicinesAsync(string? searchTerm = null);
        Task DecrementStockAsync(int inventoryId, int quantity);
    }

    public class DatabaseInventoryStockService : IInventoryStockService
    {
        private readonly ApplicationDbContext _cloudContext;
        private readonly LocalDbContext _localContext;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabaseInventoryStockService(ApplicationDbContext cloudContext, LocalDbContext localContext)
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

        private static InventoryMedicineOption ToOption(PharmacyInventory x)
        {
            return new InventoryMedicineOption(
                x.InventoryId,
                x.ItemName,
                x.StockQuantity,
                x.Unit,
                x.ExpiryDate
            );
        }

        public async Task<IReadOnlyList<InventoryMedicineOption>> GetAvailableMedicinesAsync(string? searchTerm = null)
        {
            return await WithDbLock(async () =>
            {
                var term = (searchTerm ?? string.Empty).Trim();

                try
                {
                    var q = _cloudContext.PharmacyInventory
                        .AsNoTracking()
                        .Where(i => i.IsArchived == false)
                        .Where(i => i.StockQuantity > 0);

                    if (term.Length > 0)
                    {
                        q = q.Where(i => i.ItemName.Contains(term) || (i.GenericName != null && i.GenericName.Contains(term)) || (i.BrandName != null && i.BrandName.Contains(term)));
                    }

                    var items = await q
                        .OrderBy(i => i.ItemName)
                        .ToListAsync();

                    return items.Select(ToOption).ToList();
                }
                catch (Exception ex) when (IsCloudConnectionFailure(ex))
                {
                    var q = _localContext.PharmacyInventory
                        .AsNoTracking()
                        .Where(i => i.IsArchived == false)
                        .Where(i => i.StockQuantity > 0);

                    if (term.Length > 0)
                    {
                        q = q.Where(i => i.ItemName.Contains(term) || (i.GenericName != null && i.GenericName.Contains(term)) || (i.BrandName != null && i.BrandName.Contains(term)));
                    }

                    var items = await q
                        .OrderBy(i => i.ItemName)
                        .ToListAsync();

                    return items.Select(ToOption).ToList();
                }
            });
        }

        public async Task DecrementStockAsync(int inventoryId, int quantity)
        {
            await WithDbLock(async () =>
            {
                if (inventoryId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(inventoryId));
                if (quantity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(quantity));

                Exception? cloudError = null;
                Exception? localError = null;

                try
                {
                    var item = await _cloudContext.PharmacyInventory.FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
                    if (item == null)
                        throw new InvalidOperationException("Medicine not found in cloud inventory.");

                    if (item.IsArchived)
                        throw new InvalidOperationException("Medicine is archived.");

                    if (item.StockQuantity < quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {item.StockQuantity}");

                    item.StockQuantity -= quantity;
                    item.UpdatedAt = DateTime.Now;
                    await _cloudContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    cloudError = ex;
                }

                try
                {
                    var item = await _localContext.PharmacyInventory.FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
                    if (item == null)
                        throw new InvalidOperationException("Medicine not found in local inventory.");

                    if (item.IsArchived)
                        throw new InvalidOperationException("Medicine is archived.");

                    if (item.StockQuantity < quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {item.StockQuantity}");

                    item.StockQuantity -= quantity;
                    item.UpdatedAt = DateTime.Now;
                    await _localContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    localError = ex;
                }

                if (cloudError != null && localError != null)
                {
                    throw new InvalidOperationException($"Failed to decrement stock (cloud and local). Cloud: {cloudError.Message} | Local: {localError.Message}");
                }

                if (cloudError != null && IsCloudConnectionFailure(cloudError))
                {
                    if (localError != null)
                        throw new InvalidOperationException($"Failed to decrement stock locally: {localError.Message}");
                }
            });
        }
    }
}
