using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    public class FakeSignInManager : SignInManager<ApplicationUser>
    {
        public FakeSignInManager(ApplicationUser[] existingUsers, IPasswordValidator<ApplicationUser> passwordValidator, IOptions<IdentityOptions> options)
            : base(new FakeUserManager(existingUsers, passwordValidator),
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                options,
                new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<ApplicationUser>>().Object)
        { }
    }
}
