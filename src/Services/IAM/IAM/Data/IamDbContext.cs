using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAM.Data;

public class IamDbContext : IdentityDbContext
{
    public IamDbContext(DbContextOptions<IamDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName()!;
            if (tableName.StartsWith("Aspnet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
    }
}