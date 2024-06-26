using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportAccountValidator : AbstractValidator<ImportRecord<ImportableContact>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator;
        private readonly IMemberService _memberService;
        private readonly IStoreService _storeService;
        private readonly ISettingsManager _settingsManager;
        private readonly ImportRecord<ImportableContact>[] _allRecords;

        public ImportAccountValidator(UserManager<ApplicationUser> userManager, IPasswordValidator<ApplicationUser> passwordValidator, IMemberService memberService, IStoreService storeService, ISettingsManager settingsManager, ImportRecord<ImportableContact>[] allRecords)
        {
            _userManager = userManager;
            _passwordValidator = passwordValidator;
            _memberService = memberService;
            _storeService = storeService;
            _settingsManager = settingsManager;
            _allRecords = allRecords;

            When(x => x.Record.AdditionalLine != true
                      && Array.Exists([
                          x.Record.AccountType, x.Record.AccountStatus, x.Record.AccountLogin,
                          x.Record.AccountEmail, x.Record.StoreId, x.Record.StoreName,
                          x.Record.EmailVerified.ToString(),
                      ], field => !string.IsNullOrEmpty(field)),
                AttachValidators);
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.AccountLogin)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Account Login")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Record.AccountLogin)
                        .Must(userName => string.IsNullOrEmpty(_userManager.Options.User.AllowedUserNameCharacters) ||
                                          userName.All(c => _userManager.Options.User.AllowedUserNameCharacters.Contains(c)))
                        .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                        .WithMessage((_, userName) =>
                            {
                                var invalidCharacters = string.Join(null, userName.Where(c => !_userManager.Options.User.AllowedUserNameCharacters.Contains(c)));
                                return $"This row has invalid value in the column 'Account Login'. Invalid characters: '{invalidCharacters}'";
                            })
                        .WithImportState();
                    RuleFor(x => x.Record.AccountLogin)
                        .MustAsync(async (thisRecord, userName, _) =>
                        {
                            var lastRecordWithAccountLogin = _allRecords
                                .Where(x => x.Record.AdditionalLine != true)
                                .LastOrDefault(otherRecord => userName.EqualsInvariant(otherRecord.Record.AccountLogin));
                            var existedAccount = await _userManager.FindByNameAsync(userName);
                            return (existedAccount == null || await IsSameContact(existedAccount, thisRecord.Record))
                                && (_allRecords
                                       .Where(x => x.Record.AdditionalLine != true)
                                       .All(otherRecord => !userName.EqualsInvariant(otherRecord.Record.AccountLogin))
                                    || lastRecordWithAccountLogin == thisRecord);
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
                                .MustAsync(async (thisRecord, email, _) =>
                                {
                                    var lastRecordWithAccountEmail = _allRecords
                                        .Where(x => x.Record.AdditionalLine != true)
                                        .LastOrDefault(otherRecord => email.EqualsInvariant(otherRecord.Record.AccountEmail));
                                    var existedAccount = await _userManager.FindByEmailAsync(email);
                                    return (existedAccount == null || await IsSameContact(existedAccount, thisRecord.Record)) &&
                                           (_allRecords
                                                .Where(x => x.Record.AdditionalLine != true)
                                                .All(otherRecord => !email.EqualsInvariant(otherRecord.Record.AccountEmail))
                                            || lastRecordWithAccountEmail == thisRecord);
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
                        .MustAsync(ValidateStoreAsync)
                        .WithInvalidValueCodeAndMessage("Store Id")
                        .WithImportState();
                });

            RuleFor(x => x.Record.Password)
                .MustAsync(async (thisRecord, password, _) =>
                {
                    var contact = new Contact();
                    thisRecord.Record.PatchModel(contact);
                    return await _passwordValidator.ValidateAsync(_userManager, contact.SecurityAccounts.FirstOrDefault(), password) == IdentityResult.Success;
                })
                .When(x => !string.IsNullOrEmpty(x.Record.Password))
                .WithErrorCode(ModuleConstants.ValidationErrors.PasswordDoesntMeetSecurityPolicy)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.PasswordDoesntMeetSecurityPolicy], "Password"))
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
                    var accountStatuses = await _settingsManager.GetObjectSettingAsync(PlatformConstants.Settings.Security.AccountStatuses.Name);
                    return accountStatuses.AllowedValues.Contains(accountStatus);
                })
                .When(x => !string.IsNullOrEmpty(x.Record.AccountStatus))
                .WithInvalidValueCodeAndMessage("Account Status")
                .WithImportState();
        }

        private async Task<bool> IsSameContact(ApplicationUser account, ImportableContact importRecord)
        {
            if (string.IsNullOrEmpty(account?.MemberId))
            {
                return false;
            }

            var contact = await _memberService.GetByIdAsync(account.MemberId, nameof(MemberResponseGroup.Default), nameof(Contact)) as Contact;
            return contact?.FullName.EqualsInvariant(importRecord.ContactFullName) == true
                   && (contact.Id.EqualsInvariant(importRecord.Id)
                       || (!string.IsNullOrEmpty(contact.OuterId) && contact.OuterId.EqualsInvariant(importRecord.OuterId)));
        }

        private async Task<bool> ValidateStoreAsync(ImportRecord<ImportableContact> record, string storeId, CancellationToken _)
        {
            var store = await _storeService.GetNoCloneAsync(storeId, nameof(StoreResponseGroup.None));

            if (store is null)
            {
                return false;
            }

            // Fix potential case difference
            record.Record.StoreId = store.Id;

            return true;
        }
    }
}
