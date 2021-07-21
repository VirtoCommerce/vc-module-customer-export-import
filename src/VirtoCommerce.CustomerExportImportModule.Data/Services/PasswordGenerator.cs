using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerExportImportModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    /// <summary>
    /// Based on https://github.com/Darkseal/PasswordGenerator
    /// Apache License 2.0 https://github.com/Darkseal/PasswordGenerator/blob/master/LICENSE
    /// </summary>
    public class PasswordGenerator : IPasswordGenerator
    {
        private static readonly string[] RandomChars  = {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // uppercase 
            "abcdefghijklmnopqrstuvwxyz", // lowercase
            "0123456789", // digits
            "!@$?_-" // non-alphanumeric
        };

        private readonly PasswordOptions _passwordOptions;

        public PasswordGenerator(IOptions<IdentityOptions> identityOptions)
        {
            _passwordOptions = identityOptions.Value.Password;
        }

        public string GeneratePassword()
        {
            var rand = new CryptoRandom();
            var chars = new List<char>();

            if (_passwordOptions.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count), RandomChars[0][rand.Next(0, RandomChars[0].Length)]);
            }

            if (_passwordOptions.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count), RandomChars[1][rand.Next(0, RandomChars[1].Length)]);
            }

            if (_passwordOptions.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count), RandomChars[2][rand.Next(0, RandomChars[2].Length)]);
            }

            if (_passwordOptions.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count), RandomChars[3][rand.Next(0, RandomChars[3].Length)]);
            }

            for (var i = chars.Count; i < _passwordOptions.RequiredLength || chars.Distinct().Count() < _passwordOptions.RequiredUniqueChars; i++)
            {
                var rcs = RandomChars[rand.Next(0, RandomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
