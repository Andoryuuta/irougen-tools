using IrougenTools.Core;
using JetBrains.Annotations;

namespace IrougenTools.Core.Tests;

[TestSubject(typeof(KfcArchive))]
public class KfcArchiveTest
{

    [Fact]
    public void TestParse()
    {
        // We unfortunately don't have any kfc files that we can ship with our unit tests here,
        // as the original are proprietary and we can't yet (re)write our own (+original files are massive).
        var archive = new KfcArchive();
        archive.Parse(@"C:\Program Files (x86)\Steam\steamapps\common\Enshrouded\enshrouded.kfc");
        // Assert.Equal("637515|^/game38/branches/ea_update_05|2025-01-29T16:45:54.448600Z", archive.SvnVersion);
        Assert.Equal(637515, archive.SvnVersion!.Version);
        Assert.Equal("^/game38/branches/ea_update_05", archive.SvnVersion!.Branch);
        Assert.Equal(DateTimeOffset.Parse("2025-01-29T16:45:54.448600Z"), archive.SvnVersion!.Timestamp);
        
        Assert.Equal(32, archive.ContainerInfoEntries.Count);
        Assert.Equal<uint>(1084727296, archive.ContainerInfoEntries[31].ContainerSize);
        
        Assert.Single(archive.ResourceBundleEntries);
        Assert.Equal(65354, archive.ResourcePriorityEntries.Count);
        Assert.Equal(131072, archive.ContentLookupTableEntries.Count);
        Assert.Equal(106304, archive.ContentInfoEntries.Count);
        Assert.Equal(106304, archive.ContentLocationEntries.Count);
        Assert.Equal(65536, archive.ResourceLookupTableEntries.Count);
        Assert.Equal(65354, archive.ResourceInfoEntries.Count);
        Assert.Equal(65354, archive.ResourceLocationEntries.Count);
        Assert.Equal(128, archive.ReflectionTypeLookupTableEntries.Count);
        Assert.Equal(118, archive.ReflectionTypeNameHashEntries.Count);
        Assert.Equal(118, archive.ReflectionTypeInfoEntries.Count);
    }
}