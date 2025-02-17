using System.Text;
using System.Xml.Serialization;
using IrougenTools.Core.Reflection;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace IrougenTools.Core.Tests.Reflection;

[TestSubject(typeof(ReflectionSerializer))]
public class ReflectionSerializerTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ReflectionSerializerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // Local struct definition
    struct ResourceEntryPair(KfcArchive.ResourceLocation location, KfcArchive.ResourceInfo info)
    {
        public KfcArchive.ResourceLocation Location { get; } = location;
        public KfcArchive.ResourceInfo Info { get; } = info;
    }

    [Fact]
    public void TestParseResources()
    {
        // TODO: Handmade resources for unit tests, rather than game data.
        using var archiveFile = new BinaryReader(File.OpenRead(@"C:\Program Files (x86)\Steam\steamapps\common\Enshrouded\enshrouded.kfc"));
        var archive = new KfcArchive();
        archive.Parse(archiveFile);
        
        var serializer = new ReflectionSerializer();
        serializer.LoadTypeDefinitions(@"C:\RE\Games\keen\keengames\enshrouded\irougen-tools\game-data\reflection_info.637515.json");

        // Create paired entries
        var pairedEntries = archive.ResourceLocationEntries
            .Zip(archive.ResourceInfoEntries, (loc, info) => new ResourceEntryPair(loc, info))
            .ToList();

        // Sort by Size (smallest first)
        var sortedEntries = pairedEntries
            .OrderBy(pair => pair.Location.Size)
            .ToList();

        // Now you can access the sorted pairs
        foreach (var pair in sortedEntries)
        {
            var resourceName = $"{pair.Info.Guid.ToString()}.{pair.Info.TypeNameHash}";
            // if (resourceName != "cd861d95-a79c-4c11-8b48-8fae34a4156c.4135415747")
            // {
            //     // _testOutputHelper.WriteLine($"Skipping {resourceName}");
            //     continue;
            // }
            _testOutputHelper.WriteLine($"Parsing resource {resourceName}");
            
            // TODO: move this into the KfcArchive class as a "GetResourceReader" method.
            var absOffset = pair.Location.Offset + archive.ResourceBundleEntries[0].FileOffsetStart;
            archiveFile.BaseStream.Seek(absOffset, SeekOrigin.Begin);
            byte[] resourceBytes = archiveFile.ReadBytes((int)pair.Location.Size);
            var resourceReader = new BinaryReader(new MemoryStream(resourceBytes));

            var deserializedNode = serializer.Deserialize(resourceReader,
                pair.Info.TypeNameHash);

            
            // XmlSerializer xmlSerializer = new XmlSerializer(typeof(ReflectionNode));
            // StringWriter stream = new StringWriter();
            // xmlSerializer.Serialize(stream, deserializedNode);
            //
            // _testOutputHelper.WriteLine(stream.ToString());
            _testOutputHelper.WriteLine("Parsed resource OK");
            // break;
        }
    }
}