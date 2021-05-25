using System;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportedFileInfo : ICloneable
    {
        public string FileName { get; set; }
        public string FileUrl { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
