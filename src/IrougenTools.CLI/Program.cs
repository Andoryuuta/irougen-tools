using System.CommandLine;
using IrougenTools.Core;

namespace IrougenTools.CLI;

internal static class Program
{
    private const string DefaultOutputDirectory = "./output";
    private const string ResourceOutputSubdirectory = "resources";
    private const string ContentOutputDirectory = "content";
    
    private static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Tool for unpacking Enshrouded KFC2 files (.kfc)");

        var infoCommand = new Command("info", "Display information about KFC2 files (for debugging)");
        var infoInputFileOption = new Option<FileInfo>(
            name: "--input",
            description: "The .kfc file to inspect")
        {
            IsRequired = true
        };
        infoCommand.AddOption(infoInputFileOption);
        infoCommand.SetHandler(HandleInfoCommand, infoInputFileOption);
        
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

        // Parent commands
        rootCommand.AddCommand(infoCommand);
        rootCommand.AddCommand(unpackCommand);
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

            // Extract all resources to disk
            var resourceOutputPath = Path.Combine(output.FullName, ResourceOutputSubdirectory);
            Directory.CreateDirectory(resourceOutputPath);
            for (var i = 0; i < archive.ResourceInfoEntries.Count; i++)
            {
                var info = archive.ResourceInfoEntries[i];
                var location = archive.ResourceLocationEntries[i];
                var absOffset = location.Offset + archive.ResourceBundleEntries[0].FileOffsetStart;
                
                
                // Build filename from GUID/part/type hash.
                // We don't need to include the reserved fields as they should always be 0.
                var filename = $"{info.Guid.ToString()}.{info.PartIndex}.{info.TypeNameHash:X8}";
                
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
}