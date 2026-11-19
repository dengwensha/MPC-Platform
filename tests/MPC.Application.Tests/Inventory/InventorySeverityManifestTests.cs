using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Application.Tests.Inventory;

public sealed class InventorySeverityManifestTests
{
    private static readonly IReadOnlyDictionary<string, CadDiagnosticSeverity> Manifest =
        new Dictionary<string, CadDiagnosticSeverity>(StringComparer.Ordinal)
        {
            ["STEP_FILE_NOT_FOUND"] = CadDiagnosticSeverity.Fatal,
            ["STEP_FILE_EXTENSION_UNSUPPORTED"] = CadDiagnosticSeverity.Fatal,
            ["STEP_FILE_READ_FAILED"] = CadDiagnosticSeverity.Fatal,
            ["STEP_DATA_SECTION_NOT_FOUND"] = CadDiagnosticSeverity.Fatal,
            ["STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND"] = CadDiagnosticSeverity.Error,
            ["STEP_DATA_ENTITY_HEADER_UNRECOGNIZED"] = CadDiagnosticSeverity.Error,
            ["STEP_DUPLICATE_SOURCE_ENTITY_ID"] = CadDiagnosticSeverity.Error,
            ["STEP_MISSING_ENTITY_REFERENCE"] = CadDiagnosticSeverity.Error
        };

    [Fact]
    public void EntireProducerSourceAndCodeInventory_ArePinnedForReview()
    {
        var expectedHashes = new Dictionary<string, string>
        {
            ["StepDataEntityReader.cs"] = "4BAECFE943084CECBDB8FF4FB561F11ECCCA8A895B33A122081DAAB8D6F68532",
            ["StepDataInventoryValidator.cs"] = "FB5869D3EB0FE8B5507F4041A221CBC2DB88FE16917C5BC7DA7472B6771B7301"
        };
        var codes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in expectedHashes)
        {
            string source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "InventoryAudit", item.Key + ".txt"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);
            // Hashing the entire normalized producer fails even for new construction
            // styles, computed codes, helper calls or severity changes that regex
            // cannot understand. Updating this pin requires eligibility-impact review.
            Assert.Equal(item.Value, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))));
            foreach (Match match in Regex.Matches(source, "\"(STEP_[A-Z_]+)\"", RegexOptions.CultureInvariant))
                codes.Add(match.Groups[1].Value);
        }
        Assert.Equal(Manifest.Keys.OrderBy(x => x, StringComparer.Ordinal), codes.OrderBy(x => x, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("DATA; #1=(); ENDSEC;", "STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND")]
    [InlineData("DATA; nonsense; ENDSEC;", "STEP_DATA_ENTITY_HEADER_UNRECOGNIZED")]
    [InlineData("DATA; #1=A(); #1=B(); ENDSEC;", "STEP_DUPLICATE_SOURCE_ENTITY_ID")]
    [InlineData("DATA; #1=A(#99); ENDSEC;", "STEP_MISSING_ENTITY_REFERENCE")]
    [InlineData("not a DATA section", "STEP_DATA_SECTION_NOT_FOUND")]
    public void ActualReaderBranches_PreservePinnedSeverity(string text, string expectedCode)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try
        {
            File.WriteAllText(path, text);
            var result = new StepDataEntityReader(new StepDataInventoryValidator()).Read(path);
            var diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(expectedCode, diagnostic.Code);
            Assert.Equal(Manifest[expectedCode], diagnostic.Severity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void FileFailureBranches_PreservePinnedFatalSeverity()
    {
        var reader = new StepDataEntityReader(new StepDataInventoryValidator());
        string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        var d = Assert.Single(reader.Read(missing).Diagnostics);
        Assert.Equal("STEP_FILE_NOT_FOUND", d.Code);
        Assert.Equal(Manifest[d.Code], d.Severity);
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        string lockedPath = Path.ChangeExtension(path, ".step");
        try
        {
            File.WriteAllText(path, "DATA; ENDSEC;");
            d = Assert.Single(reader.Read(path).Diagnostics);
            Assert.Equal("STEP_FILE_EXTENSION_UNSUPPORTED", d.Code);
            Assert.Equal(Manifest[d.Code], d.Severity);
            File.Copy(path, lockedPath);
            using var locked = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            d = Assert.Single(reader.Read(lockedPath).Diagnostics);
            Assert.Equal("STEP_FILE_READ_FAILED", d.Code);
            Assert.Equal(Manifest[d.Code], d.Severity);
        }
        finally { File.Delete(path); File.Delete(lockedPath); }
    }
}
