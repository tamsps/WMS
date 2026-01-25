using Microsoft.EntityFrameworkCore;
using WMS.Domain.Data;

namespace WMS.Tests.Fixtures;

/// <summary>
/// Test database context factory for in-memory testing
/// Creates isolated in-memory databases for each test
/// </summary>
public class TestDbContextFactory
{
    public static WMSDbContext CreateInMemoryContext(string databaseName = "")
    {
        var dbName = string.IsNullOrEmpty(databaseName) 
            ? $"WMSTest_{Guid.NewGuid()}" 
            : databaseName;

        var options = new DbContextOptionsBuilder<WMSDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new WMSDbContext(options);
        context.Database.EnsureCreated();
        
        return context;
    }

    public static void SeedTestData(WMSDbContext context)
    {
        // Add common test data here if needed
        context.SaveChanges();
    }
}
