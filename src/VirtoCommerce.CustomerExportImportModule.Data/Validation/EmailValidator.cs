using FluentValidation;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class EmailValidator : AbstractValidator<string>
    {
        public EmailValidator()
        {
            RuleFor(x => x).EmailAddress();
        }
    }
}
