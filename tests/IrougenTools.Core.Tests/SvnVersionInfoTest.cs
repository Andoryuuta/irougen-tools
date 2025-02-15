using IrougenTools.Core;
using JetBrains.Annotations;

namespace IrougenTools.Core.Tests;

[TestSubject(typeof(SvnVersionInfo))]
public class SvnVersionInfoTest
{

    [Fact]
    public void TestParseSvnVersionInfo()
    {
        SvnVersionInfo versionInfo = SvnVersionInfo.Parse("637515|^/game38/branches/ea_update_05|2025-01-29T16:45:54.448600Z");
        Assert.Equal(637515, versionInfo.Version);
        Assert.Equal("^/game38/branches/ea_update_05", versionInfo.Branch);
        Assert.Equal(DateTimeOffset.Parse("2025-01-29T16:45:54.448600Z"), versionInfo.Timestamp);
    }
    
    [Fact]
    public void TestSvnVersionInfoReformat()
    {
        var versionString = "637515|^/game38/branches/ea_update_05|2025-01-29T16:45:54.448600Z";
        SvnVersionInfo versionInfo = SvnVersionInfo.Parse(versionString);
        Assert.Equal(versionString, versionInfo.ToString());
    }
}