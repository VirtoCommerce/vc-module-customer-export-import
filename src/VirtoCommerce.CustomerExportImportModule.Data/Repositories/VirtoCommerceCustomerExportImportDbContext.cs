using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CustomerExportImportModule.Data.Repositories
{
    public class VirtoCommerceCustomerExportImportDbContext : DbContextBase
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

