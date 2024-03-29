using System.Collections.Generic;
using System.Collections.ObjectModel;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerExportImportModule.Core
{
    public static class ModuleConstants
    {
        public const int KByte = 1024;

        public const int MByte = 1024 * KByte;

        public const string DefaultContactAccountPassword = "Password1!";

        public static class ValidationErrors
        {
            public const string DuplicateError = "Duplicate";

            public const string WrongAdditionalLine = "wrong-additional-line";

            public const string FileNotExisted = "file-not-existed";

            public const string NoData = "no-data";

            public const string ExceedingFileMaxSize = "exceeding-file-max-size";

            public const string WrongDelimiter = "wrong-delimiter";

            public const string ExceedingLineLimits = "exceeding-line-limits";

            public const string MissingRequiredColumns = "missing-required-columns";

            public const string MissingRequiredValues = "missing-required-values";

            public const string ExceedingMaxLength = "exceeding-max-length";

            public const string ArrayValuesExceedingMaxLength = "array-values-exceeding-max-length";

            public const string InvalidValue = "invalid-value";

            public const string NotUniqueValue = "not-unique-value";

            public const string PasswordDoesntMeetSecurityPolicy = "password-doesnt-meet-security-policy";
        }

        public static readonly ReadOnlyDictionary<string, string> ValidationMessages = new(new Dictionary<string, string>
        {
            { ValidationErrors.MissingRequiredValues, "The required value in column '{0}' is missing." },
            { ValidationErrors.ExceedingMaxLength, "Value in column '{0}' may have maximum {1} characters." },
            { ValidationErrors.ArrayValuesExceedingMaxLength, "Every value in column '{0}' may have maximum {1} characters. The number of values is unlimited." },
            { ValidationErrors.InvalidValue, "This row has invalid value in the column '{0}'." },
            { ValidationErrors.NotUniqueValue, "Value in column '{0}' should be unique." },
            { ValidationErrors.PasswordDoesntMeetSecurityPolicy, "Password does not meet the platform security policy. Please, contact administrator" }
        });

        public static class Features
        {
            public const string CustomerExportImport = "CustomerExportImport";
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ExportAccess = "customer:simpleExport";

                public const string ImportAccess = "customer:simpleImport";

                public static string[] AllPermissions { get; } = { ExportAccess, ImportAccess };
            }
        }

        public static class Settings
        {
            public const int PageSize = 50;

            public static class General
            {
                public static SettingDescriptor ExportLimitOfLines { get; } = new()
                {
                    Name = "CustomerExportImport.Export.LimitOfLines",
                    GroupName = "CustomerExportImport|Export",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportLimitOfLines { get; } = new()
                {
                    Name = "CustomerExportImport.Import.LimitOfLines",
                    GroupName = "CustomerExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportFileMaxSize { get; } = new()
                {
                    Name = "CustomerExportImport.Import.FileMaxSize",
                    GroupName = "CustomerExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 1 // MB
                };

                public static SettingDescriptor AddressRegionStrongValidation { get; } = new()
                {
                    Name = "CustomerExportImport.Import.AddressRegionStrongValidation",
                    GroupName = "Customer|Import",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return ExportLimitOfLines;
                        yield return ImportLimitOfLines;
                        yield return ImportFileMaxSize;
                        yield return AddressRegionStrongValidation;
                    }
                }
            }
        }
    }
}
