using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IT_13FinalProject.Services
{
    public class SyncService
    {
        private readonly ApplicationDbContext _cloudContext;
        private readonly LocalDbContext _localContext;

        public SyncService(ApplicationDbContext cloudContext, LocalDbContext localContext)
        {
            _cloudContext = cloudContext;
            _localContext = localContext;
        }

        public async Task SyncToCloudAsync()
        {
            try
            {
                // Sync Users from local to cloud
                var users = await _localContext.Users.ToListAsync();
                foreach (var user in users)
                {
                    var existingUser = await _cloudContext.Users.FindAsync(user.Id);
                    if (existingUser == null)
                    {
                        _cloudContext.Users.Add(user);
                    }
                    else
                    {
                        _cloudContext.Entry(existingUser).CurrentValues.SetValues(user);
                    }
                }

                // Sync Patients from local to cloud
                var patients = await _localContext.Patients.ToListAsync();
                foreach (var patient in patients)
                {
                    var existingPatient = await _cloudContext.Patients.FindAsync(patient.PatientId);
                    if (existingPatient == null)
                    {
                        _cloudContext.Patients.Add(patient);
                    }
                    else
                    {
                        _cloudContext.Entry(existingPatient).CurrentValues.SetValues(patient);
                    }
                }

                // Sync Health Records from local to cloud
                var healthRecords = await _localContext.HealthRecords.ToListAsync();
                foreach (var record in healthRecords)
                {
                    var existingRecord = await _cloudContext.HealthRecords.FindAsync(record.RecordId);
                    if (existingRecord == null)
                    {
                        _cloudContext.HealthRecords.Add(record);
                    }
                    else
                    {
                        _cloudContext.Entry(existingRecord).CurrentValues.SetValues(record);
                    }
                }

                // Sync Vital Signs from local to cloud
                var vitalSigns = await _localContext.VitalSigns.ToListAsync();
                foreach (var vitalSign in vitalSigns)
                {
                    var existingVitalSign = await _cloudContext.VitalSigns.FindAsync(vitalSign.VitalSignId);
                    if (existingVitalSign == null)
                    {
                        _cloudContext.VitalSigns.Add(vitalSign);
                    }
                    else
                    {
                        _cloudContext.Entry(existingVitalSign).CurrentValues.SetValues(vitalSign);
                    }
                }

                // Sync Pharmacy Inventory from local to cloud
                var inventory = await _localContext.PharmacyInventory.ToListAsync();
                foreach (var item in inventory)
                {
                    var existingItem = await _cloudContext.PharmacyInventory.FindAsync(item.InventoryId);
                    if (existingItem == null)
                    {
                        _cloudContext.PharmacyInventory.Add(item);
                    }
                    else
                    {
                        _cloudContext.Entry(existingItem).CurrentValues.SetValues(item);
                    }
                }

                await _cloudContext.SaveChangesAsync();
                Console.WriteLine("Sync to cloud completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sync to cloud failed: {ex.Message}");
            }
        }

        public async Task SaveToBothAsync<T>(T entity) where T : class
        {
            try
            {
                // Save to cloud first (primary)
                _cloudContext.Add(entity);
                await _cloudContext.SaveChangesAsync();

                // Save to local as backup
                _localContext.Add(entity);
                await _localContext.SaveChangesAsync();

                Console.WriteLine($"Entity saved to both databases successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save to both databases failed: {ex.Message}");
                
                // Try to save to local if cloud fails
                try
                {
                    _localContext.Add(entity);
                    await _localContext.SaveChangesAsync();
                    Console.WriteLine($"Entity saved to local database as fallback!");
                }
                catch (Exception localEx)
                {
                    Console.WriteLine($"Local database save also failed: {localEx.Message}");
                    throw;
                }
            }
        }

        public async Task SaveToCloudOnlyAsync<T>(T entity) where T : class
        {
            try
            {
                // Save only to cloud database
                _cloudContext.Add(entity);
                await _cloudContext.SaveChangesAsync();

                Console.WriteLine($"Entity saved to cloud database successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save to cloud failed: {ex.Message}");
                throw;
            }
        }
    }
}
