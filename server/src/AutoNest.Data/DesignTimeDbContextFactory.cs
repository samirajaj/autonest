using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoNest.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AutoNestDbContext>
{
    public AutoNestDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("AUTONEST_DB_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=AutoNest;Trusted_Connection=True;TrustServerCertificate=True";

        return new AutoNestDbContext(new DbContextOptionsBuilder<AutoNestDbContext>()
            .UseSqlServer(connection).Options);
    }
}
