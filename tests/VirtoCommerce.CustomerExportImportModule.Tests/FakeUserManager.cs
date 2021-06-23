using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    public class FakeUserManager : UserManager<ApplicationUser>
    {
        private readonly ApplicationUser[] _existingUsers;

        public FakeUserManager(ApplicationUser[] existingUsers)
            : base(new Mock<IUserStore<ApplicationUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<ApplicationUser>>().Object,
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<ApplicationUser>>>().Object)
        {
            _existingUsers = existingUsers;
        }

        public override Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(_existingUsers.FirstOrDefault(user => user.UserName == userName));
        }

        public override Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return Task.FromResult(_existingUsers.FirstOrDefault(user => user.Email == email));
        }
    }
}
