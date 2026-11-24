using System.Security.Cryptography;
using System.Text;

namespace MPC.Application.Tests.Step;
public sealed class ImportUnitPrecheckManifestTests
{
    [Theory]
    [InlineData("ImportUnitPrecheck.cs","64A488249CDE753A1B06E3DF5B7259AECAC296FBEB66194B3A952F8F9DEE7E6B")]
    [InlineData("StepImportApplicationService.cs","E4ADE244C2E322FBF81D118957FE88F4BFACF291A24D14C2B8CDA22C46143F1A")]
    [InlineData("ManufacturingRegionImportService.cs","5FA8C18CBFD6B766E49BE5E984D286683F6F346B741051CA547CA9638F383BE0")]
    public void ReviewPinsGuardAndBothCallerBoundaries(string name,string expected)
    {
        string content=File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"RUUnitAudit",name)).Replace("\r\n","\n",StringComparison.Ordinal);
        Assert.Equal(expected,Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))));
    }
}
