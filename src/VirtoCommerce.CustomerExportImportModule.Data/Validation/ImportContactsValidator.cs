using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactsValidator: AbstractValidator<ImportRecord<ImportableContact>[]>
    {
        private readonly IImportMemberValidator<ImportableContact> _memberValidator;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator;
        private readonly IMemberService _memberService;
        private readonly IStoreSearchService _storeSearchService;
        private readonly ISettingsManager _settingsManager;

        public ImportContactsValidator(
            IImportMemberValidator<ImportableContact> memberValidator,
            SignInManager<ApplicationUser> signInManager,
            IPasswordValidator<ApplicationUser> passwordValidator,
            IMemberService memberService,
            IStoreSearchService storeSearchService,
            ISettingsManager settingsManager)
        {
            _memberValidator = memberValidator;
            _signInManager = signInManager;
            _passwordValidator = passwordValidator;
            _memberService = memberService;
            _storeSearchService = storeSearchService;
            _settingsManager = settingsManager;

            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<ImportableContact>());
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAdditionalLinesValidator<ImportableContact>());
            RuleForEach(importRecords => importRecords).SetValidator(_memberValidator);
            RuleForEach(importRecords => importRecords).SetValidator(allRecords => new ImportContactValidator(_signInManager.UserManager, _passwordValidator, _memberService, _storeSearchService, _settingsManager, allRecords));
        }
    }
}
