using System.CommandLine;
using System.Xml.Serialization;
using IrougenTools.Core;
using IrougenTools.Core.Reflection;

namespace IrougenTools.CLI;

internal static class Program
{
    private const string DefaultOutputDirectory = "./output";
    private const string ResourceOutputSubdirectory = "resources";
    private const string ContentOutputDirectory = "content";
    private const string ReflectionInfoPathTemplate = "./game-data/reflection_info.{0}.json";

    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Tool for unpacking Enshrouded KFC2 files (.kfc)");

        // Inspect KFC
        var infoCommand = new Command("info", "Display information about KFC2 files (for debugging)");
        var infoInputFileOption = new Option<FileInfo>(
            name: "--input",
            description: "The .kfc file to inspect")
        {
            IsRequired = true
        };
        infoCommand.AddOption(infoInputFileOption);
        infoCommand.SetHandler(HandleInfoCommand, infoInputFileOption);

        // Unpack KFC
        var unpackCommand = new Command("unpack", "Unpack a .kfc file");
        var unpackInputFileOption = new Option<FileInfo>(
            name: "--input",
            description: "The .kfc file to unpack")
        {
            IsRequired = true
        };
        var unpackOutputDirOption = new Option<DirectoryInfo>(
            name: "--output",
            description: "Output directory (optional)",
            getDefaultValue: () => new DirectoryInfo(DefaultOutputDirectory));
        unpackCommand.AddOption(unpackInputFileOption);
        unpackCommand.AddOption(unpackOutputDirOption);
        unpackCommand.SetHandler(HandleUnpackCommand, unpackInputFileOption, unpackOutputDirOption);
        
        // Deserialize resources
        var deserializeResourceCommand = new Command("deserialize-resource", "Deserialize a resource file to xml");
        var deserializeResourceInputFileOption = new Option<FileInfo>(
            name: "--input",
            description: "The resource file to deserialize")
        {
            IsRequired = true
        };
        var deserializeResourceReflectionFileInfoOption = new Option<FileInfo>(
            name: "--types",
            description: "The type reflection info file")
        {
            IsRequired = true
        };

        deserializeResourceCommand.AddOption(deserializeResourceInputFileOption);
        deserializeResourceCommand.AddOption(deserializeResourceReflectionFileInfoOption);
        deserializeResourceCommand.SetHandler(HandleDeserializeResourceCommand, deserializeResourceInputFileOption, deserializeResourceReflectionFileInfoOption);
        
        // Parent commands to root
        rootCommand.AddCommand(infoCommand);
        rootCommand.AddCommand(unpackCommand);
        rootCommand.AddCommand(deserializeResourceCommand);
        return await rootCommand.InvokeAsync(args);
    }

    private static void HandleInfoCommand(FileInfo input)
    {
        if (!input.Exists)
        {
            Console.Error.WriteLine($"Input file does not exist: {input.FullName}");
            return;
        }

        try
        {
            using var archiveFile = new BinaryReader(File.OpenRead(input.FullName));
            var archive = new KfcArchive();
            archive.Parse(archiveFile);

            Console.WriteLine($"SVN Version: {archive.SvnVersion.Version}");
            Console.WriteLine($"SVN Branch: {archive.SvnVersion.Branch}");
            Console.WriteLine($"SVN Timestamp: {archive.SvnVersion.Timestamp}");
            Console.WriteLine($"Container Count: {archive.ContainerInfoEntries.Count}");
            Console.WriteLine($"Resource Count: {archive.ResourceBundleEntries.First().EntryCount}");
            Console.WriteLine($"Content Count: {archive.ContentInfoEntries.Count}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inspecting file: {ex.Message}");
        }
    }

    private static void HandleUnpackCommand(FileInfo input, DirectoryInfo output)
    {
        if (!input.Exists)
        {
            Console.Error.WriteLine($"Input file does not exist: {input.FullName}");
            return;
        }

        try
        {
            using var archiveFile = new BinaryReader(File.OpenRead(input.FullName));
            var archive = new KfcArchive();
            archive.Parse(archiveFile);

            // Load reflection data to generate file extensions.
            var reflectionInfoPath = string.Format(ReflectionInfoPathTemplate, archive.SvnVersion.Version);
            if (!File.Exists(reflectionInfoPath))
            {
                // TODO: should we allow raw dumps (without the reflection data)?
                Console.Error.WriteLine(
                    $"Could not find reflection info file matching this archive version: {reflectionInfoPath}");
                return;
            }

            var typeDefinitions = TypeDefinitionLoader.LoadFromFile(reflectionInfoPath);

            // Generate list of hash1 -> "filepath-safe" file extensions. 
            var hash1ToSafeName = typeDefinitions
                .Select(td => new
                {
                    Hash = td.Hash1,
                    SafeName = td.QualifiedName
                        .Replace("::", "-")
                        .Replace("<", "_")
                        .Replace(">", "_")
                })
                .GroupBy(x => x.Hash)
                .ToDictionary(
                    g => g.Key,
                    g => g.Single().SafeName
                );

            // Extract all resources to disk
            var resourceOutputPath = Path.Combine(output.FullName, ResourceOutputSubdirectory);
            Directory.CreateDirectory(resourceOutputPath);
            for (var i = 0; i < archive.ResourceInfoEntries.Count; i++)
            {
                var info = archive.ResourceInfoEntries[i];
                var location = archive.ResourceLocationEntries[i];
                var absOffset = location.Offset + archive.ResourceBundleEntries[0].FileOffsetStart;


                // Build filename from GUID/part/type hash, with a default hex-encoded type name hash if we don't
                // have the real one from the reflection data.
                // We don't need to include the reserved fields as they should always be 0.
                var filename = $"{info.Guid.ToString()}.{info.PartIndex}.{info.TypeNameHash:X8}";
                if (hash1ToSafeName.TryGetValue(info.TypeNameHash, out var safeName))
                {
                    filename = $"{info.Guid.ToString()}.{info.PartIndex}.{safeName}";
                }

                // Console.WriteLine("Extracting resource #{0}: {1} (offset:0x{2:X}, size:0x{3:X})", i, filename, absOffset, location.Size);
                if (i % 1 == 0 || i == archive.ResourceInfoEntries.Count - 1)
                {
                    Console.WriteLine($"Extracting resource ({i}/{archive.ResourceInfoEntries.Count})");
                }

                // Write to disk
                archiveFile.BaseStream.Seek(absOffset, SeekOrigin.Begin);
                byte[] resourceBytes = archiveFile.ReadBytes((int)location.Size);
                File.WriteAllBytes(Path.Combine(output.FullName, filename), resourceBytes);

                // using (var fs = new FileStream(Path.Combine(resourceOutputPath, filename), FileMode.CreateNew, 
                //            FileAccess.Write, FileShare.None, 
                //            4096, FileOptions.None))
                // {
                //     archiveFile.BaseStream.CopyTo(fs, (int)location.Size);
                //     // fs.Write(resourceBytes, 0, resourceBytes.Length);
                // }
            }

            // await UnpackFileAsync(input, output);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error unpacking file: {ex.Message}");
        }
    }
    
    private static void HandleDeserializeResourceCommand(FileInfo inputPath, FileInfo reflectionPath)
    {
        if (!inputPath.Exists)
        {
            Console.Error.WriteLine($"Input file does not exist: {inputPath.FullName}");
            return;
        }
        
        if (!reflectionPath.Exists)
        {
            Console.Error.WriteLine($"Input file does not exist: {reflectionPath.FullName}");
            return;
        }

        try
        {
            // Load reflection data to generate file extensions.
            var typeDefinitions = TypeDefinitionLoader.LoadFromFile(reflectionPath.FullName);

            // Generate list of hash1 -> "filepath-safe" file extensions. 
            var hash1ToSafeName = typeDefinitions
                .Select(td => new
                {
                    Hash = td.Hash1,
                    SafeName = td.QualifiedName
                        .Replace("::", "-")
                        .Replace("<", "_")
                        .Replace(">", "_")
                })
                .GroupBy(x => x.Hash)
                .ToDictionary(
                    g => g.Key,
                    g => g.Single().SafeName
                );
            
            // Inverse lookup "filepath-safe" -> hash1
            var safeNameToHash1 = hash1ToSafeName
                .ToDictionary(
                    kvp => kvp.Value,
                    kvp => kvp.Key
                );

            var inputSafeName = inputPath.Name.Split(".").Last();
            if (!safeNameToHash1.TryGetValue(inputSafeName, out var hash1))
            {
                Console.WriteLine($"Failed to get type hash from input file {inputPath.Name} (type: {inputSafeName})");
            }
            
            // Create the serializer and dump
            var serializer = new ReflectionSerializer();
            serializer.LoadTypeDefinitions(reflectionPath.FullName);

            using (var resourceReader = new BinaryReader(File.OpenRead(inputPath.FullName)))
            {
                var deserializedNode = serializer.Deserialize(resourceReader, hash1);
                
                // TODO: VERY temporary xml serialization, this will be entirely changed soon.
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ReflectionNode));
                StringWriter stream = new StringWriter();
                xmlSerializer.Serialize(stream, deserializedNode);

                Console.WriteLine(stream.ToString());
            }
            
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error unpacking file: {ex.Message}");
        }
    }
    
}