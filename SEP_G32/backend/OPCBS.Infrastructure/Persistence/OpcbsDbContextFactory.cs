using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OPCBS.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations
/// </summary>
public class OpcbsDbContextFactory : IDesignTimeDbContextFactory<OpcbsDbContext>
{
    public OpcbsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OpcbsDbContext>();
        
        // Default connection string for migrations
        // In production, this would come from configuration
        var connectionString = "Server=localhost;Database=OPCBS;Trusted_Connection=True;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new OpcbsDbContext(optionsBuilder.Options);
    }
}
