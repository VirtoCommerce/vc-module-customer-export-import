using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.CustomerExportImportModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VirtoCommerceCustomerExportImportDbContext>
    {
        public VirtoCommerceCustomerExportImportDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VirtoCommerceCustomerExportImportDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new VirtoCommerceCustomerExportImportDbContext(builder.Options);
        }
    }
}
