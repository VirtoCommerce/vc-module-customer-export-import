using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests;

[Trait("Category", "CI")]
public class CsvPagedDataImporterTests
{
    [Fact]
    public void GetReportFilePath_FileNameWithSpaces_ShouldReturnCorrectPath()
    {
        const string fileNameWithSpaces = "/Folder/name%20with%20spaces.csv";

        var result = CsvPagedDataImporter<CsvMember, Member>.GetReportFilePath(fileNameWithSpaces);

        Assert.NotNull(result);
        Assert.Equal("/Folder/name with spaces_report.csv", result);
    }
}
