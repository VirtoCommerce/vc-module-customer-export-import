using System;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
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
        private readonly ImportRecord<ImportableContact>[] _allRecords;

        public ImportAccountValidator(UserManager<ApplicationUser> userManager, IStoreSearchService storeSearchService, ISettingsManager settingsManager, ImportRecord<ImportableContact>[] allRecords)
        {
            _userManager = userManager;
            _storeSearchService = storeSearchService;
            _settingsManager = settingsManager;
            _allRecords = allRecords;
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
                                .MustAsync(async (thisRecord, userName, __) =>
                                {
                                    var lastRecordWithAccountLogin = _allRecords.LastOrDefault(otherRecord => userName.EqualsInvariant(otherRecord.Record.AccountLogin));
                                    return await _userManager.FindByNameAsync(userName) == null &&
                                           (_allRecords.All(otherRecord => !userName.EqualsInvariant(otherRecord.Record.AccountLogin)) || lastRecordWithAccountLogin == thisRecord);
                                })
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
                                        .MustAsync(async (thisRecord, email, __) =>
                                        {
                                            var lastRecordWithAccountEmail = _allRecords.LastOrDefault(otherRecord => email.EqualsInvariant(otherRecord.Record.AccountEmail));
                                            return await _userManager.FindByEmailAsync(email) == null &&
                                                   (_allRecords.All(otherRecord => !email.EqualsInvariant(otherRecord.Record.AccountEmail)) || lastRecordWithAccountEmail == thisRecord);
                                        })
                                        .WithNotUniqueValueCodeAndMessage("Account Email")
                                        .WithImportState();
                                });
                        });

                    RuleFor(x => x.Record.StoreId)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Store Id")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.StoreId)
                                .MustAsync(async (storeId, _) =>
                                {
                                    var storeSearchResult = await _storeSearchService.SearchStoresAsync(new StoreSearchCriteria { StoreIds = new [] { storeId }, Take = 0 });
                                    return storeSearchResult.TotalCount == 1;
                                })
                                .WithInvalidValueCodeAndMessage("Store Id")
                                .WithImportState();
                        });

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
