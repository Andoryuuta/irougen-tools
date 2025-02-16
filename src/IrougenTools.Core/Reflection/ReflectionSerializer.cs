using System.Diagnostics;
using System.Dynamic;
using System.Text;
using IrougenTools.Core.Reflection.Models;

namespace IrougenTools.Core.Reflection;

public class ReflectionSerializer
{
    private List<TypeDefinition> _types = [];
    public Dictionary<string, TypeDefinition> _typeByQualifiedName = new();
    public Dictionary<uint, TypeDefinition> _typeByHash1 = new();

    public void LoadTypeDefinitions(string filepath)
    {
        this._types = TypeDefinitionLoader.LoadFromFile(filepath);
        this._typeByQualifiedName = _types.ToDictionary(rt => rt.QualifiedName);
        this._typeByHash1 = _types.ToDictionary(rt => rt.Hash1);
    }

    private static ulong AlignTo(ulong offset, uint alignment)
    {
        // Verify alignment is a power of 2
        if ((alignment & (alignment - 1)) != 0)
            throw new ArgumentException("Alignment must be a power of 2", nameof(alignment));

        // Subtract 1 from alignment to create mask
        // Then add that value and mask with inverse
        return (offset + (alignment - 1)) & ~((ulong)(alignment - 1));
    }

    public ReflectionNode Deserialize(ref BinaryReader reader, uint typeHash, string nodeName = "_root")
    {
        // Console.WriteLine($"Deserializing nodeName:{nodeName} at offset:{reader.BaseStream.Position}");
        if (!_typeByHash1.TryGetValue(typeHash, out var reflectionType))
        {
            throw new ArgumentException($"Unknown type for hash: {typeHash}");
        }

        var node = new ReflectionNode
        {
            Name = nodeName,
            Type = reflectionType.QualifiedName,
        };

        var objectBasePosition = reader.BaseStream.Position;
        switch (reflectionType.PrimitiveType)
        {
            case "None":
                throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}");
                break;
            case "Bool":
                node.PrimitiveValue = reader.ReadBoolean();
                break;
            case "Uint8":
                node.PrimitiveValue = reader.ReadByte();
                break;
            case "Sint8":
                node.PrimitiveValue = reader.ReadSByte();
                break;
            case "Uint16":
                node.PrimitiveValue = reader.ReadUInt16();
                break;
            case "Sint16":
                node.PrimitiveValue = reader.ReadInt16();
                break;
            case "Uint32":
                node.PrimitiveValue = reader.ReadUInt32();
                break;
            case "Sint32":
                node.PrimitiveValue = reader.ReadInt32();
                break;
            case "Uint64":
                node.PrimitiveValue = reader.ReadUInt64();
                break;
            case "Sint64":
                node.PrimitiveValue = reader.ReadInt64();
                break;
            case "Float32":
                node.PrimitiveValue = reader.ReadSingle();
                break;
            case "Float64":
                node.PrimitiveValue = reader.ReadDouble();
                break;
            case "Enum":
                // TODO: write enum string instead of int
                if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName!,
                        out var enumReferencedReflectionType))
                {
                    throw new ArgumentException(
                        $"Unknown type for qualified name: {reflectionType.ReferencedTypeName}");
                }

                var enumNode = this.Deserialize(ref reader, enumReferencedReflectionType.Hash1, "_enum");
                node.Children.Add(enumNode);
                break;
            case "Bitmask8":
                node.PrimitiveValue = reader.ReadByte();
                break;
            case "Bitmask16":
                node.PrimitiveValue = reader.ReadUInt16();
                break;
            case "Bitmask32":
                node.PrimitiveValue = reader.ReadUInt32();
                break;
            case "Bitmask64":
                node.PrimitiveValue = reader.ReadUInt64();
                break;
            case "Typedef":
                if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName!,
                        out var typedefReferencedReflectionType))
                {
                    throw new ArgumentException(
                        $"Unknown type for qualified name: {reflectionType.ReferencedTypeName}");
                }

                var typedefNode = this.Deserialize(ref reader, typedefReferencedReflectionType.Hash1, "_typedef");
                node.Children.Add(typedefNode);
                break;
            case "Struct":
                var structReadStartPosition = reader.BaseStream.Position;
                ulong structFieldBytesRead = 0;

                // Handle inheritance of base class/struct 
                if (reflectionType.ReferencedTypeName != null)
                {
                    if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName,
                            out var structReferencedReflectionType))
                    {
                        throw new ArgumentException(
                            $"Unknown type for qualified name: {reflectionType.ReferencedTypeName}");
                    }
                    
                    //
                    var fieldBaseNodeStartPosition = reader.BaseStream.Position;
                    var fieldBaseNode = this.Deserialize(ref reader, structReferencedReflectionType.Hash1, "_base");
                    node.Children.Add(fieldBaseNode);
                    var fieldBaseNodeEndPosition = reader.BaseStream.Position;
                    
                    // Abstract-ish base classes also have a fake sizes (e.g. 1 byte) with no fields/actual data,
                    // which throws off the offsets/alignment if not handled.
                    var realFieldBaseNodeSize = fieldBaseNodeEndPosition - fieldBaseNodeStartPosition;
                    if (realFieldBaseNodeSize > 1)
                    {
                        structFieldBytesRead += structReferencedReflectionType.ClassSize;
                    }
                }

                // Read fields of this struct.
                foreach (var field in reflectionType.StructFields)
                {
                    if (!_typeByQualifiedName.TryGetValue(field.TypeName, out var fieldReflectionType))
                    {
                        throw new ArgumentException($"Unknown type for qualified name: {field.TypeName}");
                    }

                    // // Align up to the next field.
                    // var alignedRead = AlignTo(structFieldBytesRead, fieldReflectionType.Alignment1);
                    // // Console.WriteLine($"Current structFieldBytesRead: {structFieldBytesRead}, aligning up to: {alignedRead} for type {fieldReflectionType.QualifiedName}");
                    // var skipBytes = alignedRead - structFieldBytesRead;
                    // reader.BaseStream.Position += (long)skipBytes;
                    // structFieldBytesRead += skipBytes;
                    //
                    // // Sanity check that our alignment logic is actually working as expected.
                    // if (structFieldBytesRead != field.DataOffset)
                    // {
                    //     throw new Exception(
                    //         $"Expected to read struct field {reflectionType.QualifiedName}.{field.Name} at offset {field.DataOffset}, but cursor is at {structFieldBytesRead}");
                    // }
                    
                    // This alignment is just too weird :(
                    // always seek directly to the defined offset instead of trying to calculate it.
                    reader.BaseStream.Position = structReadStartPosition + (long)field.DataOffset;

                    var fieldStartOffset = reader.BaseStream.Position;
                    var fieldNode = this.Deserialize(ref reader, fieldReflectionType.Hash1, field.Name);
                    node.Children.Add(fieldNode);
                    var fieldEndOffset = reader.BaseStream.Position;

                    structFieldBytesRead += (ulong)(fieldEndOffset - fieldStartOffset);
                }

                // The type itself may have padding on the end, so we always read the EXACT bytes specified for a struct
                // (if it read any data)
                if (structFieldBytesRead > 0)
                {
                    reader.BaseStream.Position = structReadStartPosition + reflectionType.ClassSize;
                }
                
                break;
            case "StaticArray":
                // Purely static inline, no seeking

                if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName!,
                        out var staticArrayReferencedReflectionType))
                {
                    throw new ArgumentException(
                        $"Unknown type for qualified name: {reflectionType.ReferencedTypeName}");
                }

                for (int i = 0; i < reflectionType.FieldsCount; i++)
                {
                    var staticArrayNode = this.Deserialize(ref reader, staticArrayReferencedReflectionType.Hash1,
                        $"_staticArray[{i}]");
                    node.Children.Add(staticArrayNode);
                }

                break;
            case "DsArray":
                throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}");
                break;
            case "DsString":
                throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}");
                break;
            case "DsOptional":
                throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}");
                break;
            case "DsVariant":
                throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}");
                break;
            case "BlobArray":
                if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName!,
                        out var blobArrayReflectionType))
                {
                    throw new ArgumentException(
                        $"Unknown type for qualified name: {reflectionType.ReferencedTypeName}");
                }

                var blobArrayBasePosition = reader.BaseStream.Position;
                var blobArrayRelativeOffset = reader.ReadUInt32();
                var blobArrayCount = reader.ReadUInt32();

                // Seek into it, read the data
                var blobArraySaveCursor = reader.BaseStream.Position;
                reader.BaseStream.Position = blobArrayBasePosition + blobArrayRelativeOffset;

                for (var i = 0; i < blobArrayCount; i++)
                {
                    var blobArrayNode = this.Deserialize(ref reader, blobArrayReflectionType.Hash1, $"_blobArray[{i}]");
                    node.Children.Add(blobArrayNode);
                }

                // Restore to _after_ our BlobArray fields.
                reader.BaseStream.Position = blobArraySaveCursor;

                break;
            case "BlobString":
                var blobStringBasePosition = reader.BaseStream.Position;
                var blobStringRelativeOffset = reader.ReadUInt32();
                var blobStringCount = reader.ReadUInt32();

                // Seek into it, read the data
                var blobStringSaveCursor = reader.BaseStream.Position;
                reader.BaseStream.Position = blobStringBasePosition + blobStringRelativeOffset;

                node.PrimitiveValue = Encoding.UTF8.GetString(reader.ReadBytes((int)blobStringCount));

                // Restore to _after_ our BlobString fields.
                reader.BaseStream.Position = blobStringSaveCursor;

                break;
            case "BlobOptional":
                var blobOptionalBasePosition = reader.BaseStream.Position;
                var blobOptionalRelativeOffset = reader.ReadUInt32();

                if (blobOptionalRelativeOffset != 0)
                {
                    // throw new UnreachableException($"Unimplemented type: {reflectionType.PrimitiveType}, value: {blobOptionalRelativeOffset}");
                    
                    // Get reflection type
                    if (!_typeByQualifiedName.TryGetValue(reflectionType.ReferencedTypeName, out var blobOptionalReferencedReflectionType))
                    {
                        throw new Exception($"Unknown type for hash: {reflectionType.ReferencedTypeName}");
                    }
                    
                    // Seek into it, read the data
                    var blobOptionalSaveCursor = reader.BaseStream.Position;
                    reader.BaseStream.Position = blobOptionalBasePosition + blobOptionalRelativeOffset;
                    
                    var blobOptionalNode =
                        this.Deserialize(ref reader, blobOptionalReferencedReflectionType.Hash1, $"_blobOptional");
                    node.Children.Add(blobOptionalNode);
                    
                    // Restore
                    reader.BaseStream.Position = blobOptionalSaveCursor;
                }
                
                break;
            case "BlobVariant":
                // Console.WriteLine($"Reading BlobVariant {nodeName} at position {reader.BaseStream.Position}");
                var blobVariantHash1 = reader.ReadUInt32();
                var blobVariantDataBasePosition = reader.BaseStream.Position;
                var blobVariantDataRelativeOffset = reader.ReadUInt32();
                var blobVariantDataSize = reader.ReadUInt32();

                if (blobVariantHash1 != 0 || blobVariantDataSize > 0)
                {
                    if (!_typeByHash1.TryGetValue(blobVariantHash1, out var blobVariantReferencedReflectionType))
                    {
                        throw new Exception($"Unknown type for blobVariantHash1: {blobVariantHash1}");
                    }

                    if (blobVariantDataSize != blobVariantReferencedReflectionType.ClassSize)
                    {
                        throw new UnreachableException(
                            $"Unexpected BlobVariant size for: {blobVariantReferencedReflectionType.QualifiedName}");
                    }

                    // Seek and read
                    var blobVariantSaveCursor = reader.BaseStream.Position;
                    reader.BaseStream.Position = blobVariantDataBasePosition + blobVariantDataRelativeOffset;

                    var blobVariantNode =
                        this.Deserialize(ref reader, blobVariantReferencedReflectionType.Hash1, $"_blobVariant");
                    node.Children.Add(blobVariantNode);

                    // Restore to _after_ the BlobVariant fields.
                    reader.BaseStream.Position = blobVariantSaveCursor;
                }
                
                break;
            case "ObjectReference":
                node.PrimitiveValue = reader.ReadBytes(16);
                break;
            case "Guid":
                node.PrimitiveValue = reader.ReadBytes(16);
                break;
        }

        return node;
    }
}