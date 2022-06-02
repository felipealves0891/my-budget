using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBudget.Core.Models;

namespace MyBudget.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}

        public DbSet<Group> Groups { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Cash> Cashes { get; set; }
    }
}
