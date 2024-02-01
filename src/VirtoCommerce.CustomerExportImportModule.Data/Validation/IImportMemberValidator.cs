using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation;

public interface IImportMemberValidator<T> : IValidator<ImportRecord<T>>
    where T : CsvMember
{
}
