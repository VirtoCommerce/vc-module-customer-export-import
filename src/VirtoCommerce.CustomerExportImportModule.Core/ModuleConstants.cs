using System.Collections.Generic;
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

            public const string CountryNameAndCodeDoesntMatch = "country-name-and-code-doesnt-match";
        }

        public static readonly Dictionary<string, string> ValidationMessages = new Dictionary<string, string>
        {
            { ValidationErrors.MissingRequiredValues, "The required value in column '{0}' is missing." },
            { ValidationErrors.ExceedingMaxLength, "Value in column '{0}' may have maximum {1} characters." },
            { ValidationErrors.ArrayValuesExceedingMaxLength, "Every value in column '{0}' may have maximum {1} characters. The number of values is unlimited." },
            { ValidationErrors.InvalidValue, "This row has invalid value in the column '{0}'." },
            { ValidationErrors.NotUniqueValue, "Value in column '{0}' should be unique." },
            { ValidationErrors.CountryNameAndCodeDoesntMatch, "The value in column Country Code is not for the country specified in Country column" }
        };

        public static class Features
        {
            public const string CustomerExportImport = "CustomerExportImport";
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ExportAccess = "customer:export";

                public const string ImportAccess = "customer:import";

                public static string[] AllPermissions { get; } = { ExportAccess, ImportAccess };
            }
        }

        public static class Settings
        {
            public const int PageSize = 50;

            public static class General
            {
                public static SettingDescriptor ExportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "CustomerExportImport.Export.LimitOfLines",
                    GroupName = "CustomerExportImport|Export",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "CustomerExportImport.Import.LimitOfLines",
                    GroupName = "CustomerExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportFileMaxSize { get; } = new SettingDescriptor
                {
                    Name = "CustomerExportImport.Import.FileMaxSize",
                    GroupName = "CustomerExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 1 // MB
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            ExportLimitOfLines,
                            ImportLimitOfLines,
                            ImportFileMaxSize
                        };
                    }
                }
            }
        }
    }
}
