using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IT_13FinalProject.Data;
using System;

namespace IT_13FinalProject.Tests
{
    public class DatabaseConnectionTest
    {
        private readonly IConfiguration _configuration;

        public DatabaseConnectionTest(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task TestConnection()
        {
            Console.WriteLine("=== DATABASE CONNECTION TEST ===");
            
            // Test 1: Check connection strings
            var onlineConnectionString = _configuration.GetConnectionString("OnlineConnection");
            var localConnectionString = _configuration.GetConnectionString("DefaultConnection");
            
            Console.WriteLine($"Online Connection: {onlineConnectionString}");
            Console.WriteLine($"Local Connection: {localConnectionString}");
            
            if (string.IsNullOrEmpty(onlineConnectionString))
            {
                Console.WriteLine("ERROR: Online connection string is missing!");
                return;
            }
            
            // Test 2: Try to connect to online database
            try
            {
                Console.WriteLine("Testing online database connection...");
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlServer(onlineConnectionString)
                    .Options;
                
                using var context = new ApplicationDbContext(options);
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"Online database can connect: {canConnect}");
                
                if (canConnect)
                {
                    // Test 3: Check if patients table exists and has data
                    try
                    {
                        var patientCount = await context.Patients.CountAsync();
                        Console.WriteLine($"Online database has {patientCount} patients");
                        
                        // Test 4: Try to create database if it doesn't exist
                        await context.Database.EnsureCreatedAsync();
                        Console.WriteLine("Database ensured created");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing patients table: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR connecting to online database: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }
            
            Console.WriteLine("=== END TEST ===");
        }
    }
}
