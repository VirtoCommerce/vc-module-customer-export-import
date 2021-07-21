using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.CustomerExportImportModule.Data.Repositories
{
    public class VirtoCommerceCustomerExportImportDbContext : DbContextWithTriggers
    {
        public VirtoCommerceCustomerExportImportDbContext(DbContextOptions<VirtoCommerceCustomerExportImportDbContext> options)
          : base(options)
        {
        }

        protected VirtoCommerceCustomerExportImportDbContext(DbContextOptions options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

