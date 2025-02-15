using System.Text;

namespace IrougenTools.Core;

public class KfcArchive
{
    public KfcArchive()
    {
        SvnVersion = new SvnVersionInfo
        {
            Version = 0,
            Branch = "",
            Timestamp = default
        };
        ContainerInfoEntries = [];
        ResourceBundleEntries = [];
        ResourcePriorityEntries = [];
        ContentLookupTableEntries = [];
        ContentInfoEntries = [];
        ContentLocationEntries = [];
        ResourceLookupTableEntries = [];
        ResourceInfoEntries = [];
        ResourceLocationEntries = [];
        ReflectionTypeLookupTableEntries = [];
        ReflectionTypeNameHashEntries = [];
        ReflectionTypeInfoEntries = [];
    }

    public struct ContainerInfo()
    {
        public uint ContainerSize = 0;
        public uint UnkField4 = 0;
        public uint EntryCount = 0;
        public uint UnkFieldC = 0;

        public ContainerInfo(BinaryReader reader) : this()
        {
            ContainerSize = reader.ReadUInt32();
            UnkField4 = reader.ReadUInt32();
            EntryCount = reader.ReadUInt32();
            UnkFieldC = reader.ReadUInt32();
        }
    }

    public struct ResourceBundle()
    {
        public uint FileOffsetStart = 0;
        public uint BundleSize = 0;
        public uint EntryCount = 0;

        public ResourceBundle(BinaryReader reader) : this()
        {
            FileOffsetStart = reader.ReadUInt32();
            BundleSize = reader.ReadUInt32();
            EntryCount = reader.ReadUInt32();
        }
    }

    public struct LookupTableEntry()
    {
        public uint MinIndex = 0;
        public uint MaxIndex = 0;

        public LookupTableEntry(BinaryReader reader) : this()
        {
            MinIndex = reader.ReadUInt32();
            MaxIndex = reader.ReadUInt32();
        }
    }

    public struct ContentInfo()
    {
        public uint ContentSize = 0;

        /// <summary>
        /// Used to index into the ContainerEntryLookupTableEntries.
        /// e.g. `ContainerEntryLookupTableEntries[UnkLookupTableID & (ContainerEntryLookupTableEntries.Size-1)];
        /// </summary>
        public ushort UnkLookupTableId;

        public ushort UnkField6;

        /// <summary>
        /// Content Hash - this matches the keen::ContentHash.hash1 field (used by Resource types to reference content)
        /// </summary>
        public uint ContentHash;

        public ushort UnkFieldC;
        public ushort UnkS7Index;

        public ContentInfo(BinaryReader reader) : this()
        {
            ContentSize = reader.ReadUInt32();
            UnkLookupTableId = reader.ReadUInt16();
            UnkField6 = reader.ReadUInt16();
            ContentHash = reader.ReadUInt32();
            UnkFieldC = reader.ReadUInt16();
            UnkS7Index = reader.ReadUInt16();
        }
    }
    
    public struct ContentLocation()
    {
        public ushort ContainerIndex;
        public ulong FileOffset;
        public uint BlockOffset;
        public byte ContentType;
        public uint BlockCount;

        public ContentLocation(BinaryReader reader) : this()
        {
            var locationData = reader.ReadUInt64();
            ContainerIndex = (ushort)(locationData >> 48);
            FileOffset = (ulong)(locationData & 0xFFFFFFFFFFFF);
            
            BlockOffset = reader.ReadUInt32();
            
            var contentData = reader.ReadUInt32();
            ContentType = (byte)(contentData >> 24);
            BlockCount = contentData & 0xFFFFFF;
        }
    }
    
    public struct ResourceInfo()
    {
        // This class technically should just have a single keen::ResourceId,
        // but not worth having a whole other class for, as it's not reused
        // anywhere else for this parser. Other uses for it will use the
        // full dynamic reflection data.

        public Guid Guid;
        public uint TypeNameHash;
        public uint PartIndex;
        public uint Reserved0;
        public uint Reserved1;
        

        public ResourceInfo(BinaryReader reader) : this()
        {
            Guid = new Guid(reader.ReadBytes(16));
            TypeNameHash = reader.ReadUInt32();
            PartIndex = reader.ReadUInt32();
            Reserved0 = reader.ReadUInt32();
            Reserved1 = reader.ReadUInt32();

            if (Reserved0 != 0 || Reserved1 != 0)
            {
                throw new InvalidDataException("Invalid reserved fields in keen::ResourceId");
            }
        }
    }
    
    public struct ResourceLocation()
    {
        /// <summary>
        /// Offset relative to the ResourceBundle.FileOffsetStart
        /// </summary>
        public uint Offset;
        public uint Size;

        public ResourceLocation(BinaryReader reader) : this()
        {
            Offset = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }
    
    public struct ReflectionTypeNameHash()
    {
        /// <summary>
        /// Type Name Hash.
        ///
        /// This is the FNV1A-32 hash of the fully qualified type name, corresponding to the `hash1` field in the
        /// reflection data.
        /// (keen::HashKey32 internally)
        /// </summary>
        public uint TypeNameHash;

        public ReflectionTypeNameHash(BinaryReader reader) : this()
        {
            TypeNameHash = reader.ReadUInt32();
        }
    }
    
    public struct ReflectionTypeInfo()
    {
        /// <summary>
        /// The source data / hash algorithm is unknown, but this corresponds to the `hash2` field in the
        /// reflection data.
        /// (keen::HashKey32 internally)
        /// </summary>
        public uint TypeHash2;

        public uint UnkField4;
        
        /// <summary>
        /// Number of resources in this resource bundle of this type. 
        /// </summary>
        public uint InstanceCount;

        public ReflectionTypeInfo(BinaryReader reader) : this()
        {
            TypeHash2 = reader.ReadUInt32();
            UnkField4 = reader.ReadUInt32();
            InstanceCount = reader.ReadUInt32();
        }
    }


    // Section 0 - SVN Version tag
    public SvnVersionInfo SvnVersion { get; set; }
    
    // Section 1 - Container Info Table
    public List<ContainerInfo> ContainerInfoEntries { get; private set; } = [];
    
    // Section 2 - Unknown/Unused
    // Section 3 - Unknown/Unused

    // Section 4 - Resource Bundle Table
    public List<ResourceBundle> ResourceBundleEntries { get; private set; } = [];
    
    // Section 5 - Resource Priority Table
    // (elements are resource table indices)
    public List<UInt32> ResourcePriorityEntries { get; private set; } = [];
    
    // Section 6 - Content Lookup Table
    public List<LookupTableEntry> ContentLookupTableEntries { get; private set; } = [];
    
    // Section 7 - Content Info Table
    public List<ContentInfo> ContentInfoEntries { get; private set; } = [];
    
    // Section 8 - Content Location Table
    public List<ContentLocation> ContentLocationEntries { get; private set; } = [];

    // Section 9 - Resource Lookup Table
    public List<LookupTableEntry> ResourceLookupTableEntries { get; private set; } = [];
    
    // Section 10 - Resource Info Table
    public List<ResourceInfo> ResourceInfoEntries { get; private set; } = [];
    
    // Section 11 - Resource Location Table
    public List<ResourceLocation> ResourceLocationEntries { get; private set; } = [];
    
    // Section 12 - Reflection Type Lookup Table
    public List<LookupTableEntry> ReflectionTypeLookupTableEntries { get; private set; } = [];
    
    // Section 13 - Reflection Type Name Hash Table
    public List<ReflectionTypeNameHash> ReflectionTypeNameHashEntries { get; private set; } = [];
    
    // Section 14 - Reflection Type Info Table
    public List<ReflectionTypeInfo> ReflectionTypeInfoEntries { get; private set; } = [];
    
    // // Resource Data Blobs
    // public List<byte[]> ResourceDataBlob { get; private set; } = [];

    public void Parse(string filePath)
    {
        using var reader = new BinaryReader(System.IO.File.OpenRead(filePath));
        Parse(reader);
    }
    public void Parse(BinaryReader reader)
    {
        var magic = reader.ReadUInt32();
        if (Encoding.ASCII.GetString(BitConverter.GetBytes(magic)) != "KFC2")
        {
            throw new InvalidDataException("Invalid file format: Magic number does not match 'KFC2'");
        }

        var dataStartOffset = reader.ReadUInt32();
        var field8 = reader.ReadUInt32();
        var fieldC = reader.ReadUInt32();

        var versionTagOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var versionTagLength = reader.ReadUInt32();

        var containerInfoTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var containerInfoTableCount = reader.ReadUInt32();

        var unkSection2RelOffset = reader.ReadUInt32();
        var unkSection2Count = reader.ReadUInt32();

        var unkSection3RelOffset = reader.ReadUInt32();
        var unkSection3Count = reader.ReadUInt32();

        var resourceBundleEntryOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var resourceBundleEntryCount = reader.ReadUInt32();

        var resourcePriorityTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var resourcePriorityTableCount = reader.ReadUInt32();

        var contentLookupTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var contentLookupTableCount = reader.ReadUInt32();

        var contentInfoTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var contentInfoTableCount = reader.ReadUInt32();

        var contentLocationTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var contentLocationTableCount = reader.ReadUInt32();

        var resourceLookupTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var resourceLookupTableCount = reader.ReadUInt32();

        var resourceInfoTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var resourceInfoTableCount = reader.ReadUInt32();
        
        var resourceLocationTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var resourceLocationTableCount = reader.ReadUInt32();
        
        var reflectionTypeLookupTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var reflectionTypeLookupTableCount = reader.ReadUInt32();
        
        var reflectionTypeNameHashTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var reflectionTypeNameHashTableCount = reader.ReadUInt32();
        
        var reflectionTypeInfoTableOffset = reader.BaseStream.Position + reader.ReadUInt32();
        var reflectionTypeInfoTableCount = reader.ReadUInt32();

        // Section 0 - SVN Version tag
        reader.BaseStream.Position = versionTagOffset;
        var svnVersionString = Encoding.UTF8.GetString(reader.ReadBytes((int)versionTagLength));
        this.SvnVersion = SvnVersionInfo.Parse(svnVersionString);
        
        // Section 1 - Container Info Table
        reader.BaseStream.Position = containerInfoTableOffset;
        this.ContainerInfoEntries = [];
        for (var i = 0; i < containerInfoTableCount; i++)
        {
            var entry = new ContainerInfo(reader);
            ContainerInfoEntries.Add(entry);
        }

        // Section 2 - Unknown/Unused
        if (unkSection2RelOffset != 0 || unkSection2Count != 0)
        {
            throw new InvalidDataException("Cannot parse KFC2 file with unknown section2");
        }

        // Section 3 - Unknown/Unused
        if (unkSection3RelOffset != 0 || unkSection3Count != 0)
        {
            throw new InvalidDataException("Cannot parse KFC2 file with unknown section3");
        }

        // Section 4 - Resource Bundle Table
        reader.BaseStream.Position = resourceBundleEntryOffset;
        this.ResourceBundleEntries = [];
        for (var i = 0; i < resourceBundleEntryCount; i++)
        {
            ResourceBundleEntries.Add(new ResourceBundle(reader));
        }
        if (ResourceBundleEntries.Count != 1)
        {
            throw new InvalidDataException("Cannot parse KFC2 file with more than 1 resource bundle");
        }
        
        // Section 5 - Resource Priority Table
        reader.BaseStream.Position = resourcePriorityTableOffset;
        this.ResourcePriorityEntries = [];
        for (var i = 0; i < resourcePriorityTableCount; i++)
        {
            ResourcePriorityEntries.Add(reader.ReadUInt32());
        }

        // Section 6 - Content Lookup Table
        reader.BaseStream.Position = contentLookupTableOffset;
        this.ContentLookupTableEntries = [];
        for (var i = 0; i < contentLookupTableCount; i++)
        {
            ContentLookupTableEntries.Add(new LookupTableEntry(reader));
        }
        
        // Section 7 - Content Info Table
        reader.BaseStream.Position = contentInfoTableOffset;
        this.ContentInfoEntries = [];
        for (var i = 0; i < contentInfoTableCount; i++)
        {
            ContentInfoEntries.Add(new ContentInfo(reader));
        }
        
        // Section 8 - Content Location Table
        reader.BaseStream.Position = contentLocationTableOffset;
        this.ContentLocationEntries = [];
        for (var i = 0; i < contentLocationTableCount; i++)
        {
            ContentLocationEntries.Add(new ContentLocation(reader));
        }

        // Section 9 - Resource Lookup Table
        reader.BaseStream.Position = resourceLookupTableOffset;
        this.ResourceLookupTableEntries = [];
        for (var i = 0; i < resourceLookupTableCount; i++)
        {
            ResourceLookupTableEntries.Add(new LookupTableEntry(reader));
        }
        
        // Section 10 - Resource Info Table
        reader.BaseStream.Position = resourceInfoTableOffset;
        this.ResourceInfoEntries = [];
        for (var i = 0; i < resourceInfoTableCount; i++)
        {
            ResourceInfoEntries.Add(new ResourceInfo(reader));
        }
        
        // Section 11 - Resource Location Table
        reader.BaseStream.Position = resourceLocationTableOffset;
        this.ResourceLocationEntries = [];
        for (var i = 0; i < resourceLocationTableCount; i++)
        {
            ResourceLocationEntries.Add(new ResourceLocation(reader));
        }
        
        // Section 12 - Reflection Type Lookup Table
        reader.BaseStream.Position = reflectionTypeLookupTableOffset;
        this.ReflectionTypeLookupTableEntries = [];
        for (var i = 0; i < reflectionTypeLookupTableCount; i++)
        {
            ReflectionTypeLookupTableEntries.Add(new LookupTableEntry(reader));
        }
        
        // Section 13 - Reflection Type Name Hash Table
        reader.BaseStream.Position = reflectionTypeNameHashTableOffset;
        this.ReflectionTypeNameHashEntries = [];
        for (var i = 0; i < reflectionTypeNameHashTableCount; i++)
        {
            ReflectionTypeNameHashEntries.Add(new ReflectionTypeNameHash(reader));
        }
    
        // Section 14 - Reflection Type Info Table
        reader.BaseStream.Position = reflectionTypeInfoTableOffset;
        this.ReflectionTypeInfoEntries = [];
        for (var i = 0; i < reflectionTypeInfoTableCount; i++)
        {
            ReflectionTypeInfoEntries.Add(new ReflectionTypeInfo(reader));
        }
        
        // // Resource Data Blobs
        // // TODO: Read on-demand instead? It's only ~150mb, so not _too_bad - but could be better.
        // this.ResourceDataBlob = [];
        // var resourceBundleStartOffset = ResourceBundleEntries[0].FileOffsetStart;
        // foreach (var loc in ResourceLocationEntries)
        // {
        //     var resourceDataOffset = resourceBundleStartOffset + loc.Offset;
        //     // Console.WriteLine("Reading resource data blob @{0:X}, Size:{1:X}", resourceDataOffset, loc.Size);
        //     reader.BaseStream.Position = resourceDataOffset;
        //     this.ResourceDataBlob.Add(reader.ReadBytes((int)loc.Size));
        // }
    }
}