using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAM.Data;

public class IamDbContext : IdentityDbContext
{
    public IamDbContext(DbContextOptions<IamDbContext> options)
        : base(options)
    {
    }
}