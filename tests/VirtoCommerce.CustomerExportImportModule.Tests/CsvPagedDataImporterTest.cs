using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests;

[Trait("Category", "CI")]
public class CsvPagedContactDataImporterTest
{
    [Fact]
    public void Validate_GetReportFilePathwithspacesInFileName()
    {
        var fileNameWithSpaces = "c:\\file%20with%20spaces.txt";

        var result = CsvPagedDataImporter<ImportableContact, Contact>.GetReportFilePath(fileNameWithSpaces);

        Assert.NotNull(result);
        Assert.Equal("c:\\file with spaces_report.txt", result);
    }
}

