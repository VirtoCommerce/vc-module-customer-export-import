using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportAccountValidator : AbstractValidator<ImportRecord<ImportableContact>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStoreSearchService _storeSearchService;
        private readonly ISettingsManager _settingsManager;

        public ImportAccountValidator(UserManager<ApplicationUser> userManager, IStoreSearchService storeSearchService, ISettingsManager settingsManager)
        {
            _userManager = userManager;
            _storeSearchService = storeSearchService;
            _settingsManager = settingsManager;
            AttachValidators();
        }

        private void AttachValidators()
        {
            When(x => new[]
                {
                    x.Record.AccountId, x.Record.AccountType, x.Record.AccountStatus, x.Record.AccountLogin, x.Record.AccountEmail, x.Record.StoreId, x.Record.StoreName,
                    x.Record.EmailVerified.ToString()
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {

                    RuleFor(x => x.Record.AccountLogin)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Account Login")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AccountLogin)
                                .MustAsync(async (_, userName, __) => await _userManager.FindByNameAsync(userName) == null)
                                .WithNotUniqueValueCodeAndMessage("Account Login")
                                .WithImportState();
                        });

                    RuleFor(x => x.Record.AccountEmail)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Account Email")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AccountEmail)
                                .EmailAddress()
                                .WithInvalidValueCodeAndMessage("Account Email")
                                .WithImportState().DependentRules(() =>
                                {
                                    RuleFor(x => x.Record.AccountEmail)
                                        .MustAsync(async (_, email, __) => await _userManager.FindByEmailAsync(email) == null)
                                        .WithNotUniqueValueCodeAndMessage("Account Email")
                                        .WithImportState();
                                });
                        });

                    RuleFor(x => x.Record.StoreId)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Store Id")
                        .WithImportState();
                    RuleFor(x => x.Record.StoreId)
                        .MustAsync(async (storeId, _) =>
                        {
                            var storeSearchResult = await _storeSearchService.SearchStoresAsync(new StoreSearchCriteria { StoreIds = new [] { storeId }, Take = 0 });
                            return storeSearchResult.TotalCount == 1;
                        })
                        .WithInvalidValueCodeAndMessage("Store Id")
                        .WithImportState();

                    RuleFor(x => x.Record.AccountType)
                        .MustAsync(async (accountType, _) =>
                        {
                            var accountTypes = await _settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.Security.SecurityAccountTypes.Name);
                            return accountTypes.AllowedValues.Contains(accountType);
                        })
                        .When(x => !string.IsNullOrEmpty(x.Record.AccountType))
                        .WithInvalidValueCodeAndMessage("Account Type")
                        .WithImportState();

                    RuleFor(x => x.Record.AccountStatus)
                        .MustAsync(async (accountStatus, _) =>
                        {
                            var accountStatuses = await _settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.Other.AccountStatuses.Name);
                            return accountStatuses.AllowedValues.Contains(accountStatus);
                        })
                        .When(x => !string.IsNullOrEmpty(x.Record.AccountStatus))
                        .WithInvalidValueCodeAndMessage("Account Status")
                        .WithImportState();
                });
        }
    }
}
