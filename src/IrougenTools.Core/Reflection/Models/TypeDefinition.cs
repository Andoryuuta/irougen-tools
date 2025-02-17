using System.Collections.Generic;
using Newtonsoft.Json;

namespace IrougenTools.Core.Reflection.Models
{
    public class TypeDefinition
    {
        [JsonProperty("type_name_1")]
        public required string TypeName1 { get; set; }

        [JsonProperty("type_name_2")]
        public required string TypeName2 { get; set; }

        [JsonProperty("qualified_name")]
        public required string QualifiedName { get; set; }

        [JsonProperty("namespace_tree")]
        public required NamespaceTree NamespaceTree { get; set; }

        [JsonProperty("referenced_type_name")]
        public string? ReferencedTypeName { get; set; }

        [JsonProperty("class_size")]
        public uint ClassSize { get; set; }

        [JsonProperty("alignment_1")]
        public uint Alignment1 { get; set; }

        [JsonProperty("alignment_2")]
        public uint Alignment2 { get; set; }

        [JsonProperty("fields_count")]
        public uint FieldsCount { get; set; }

        [JsonProperty("primitive_type")]
        public required string PrimitiveType { get; set; }

        [JsonProperty("field_4D")]
        public uint Field4D { get; set; }

        [JsonProperty("field_4E")]
        public uint Field4E { get; set; }

        [JsonProperty("field_4F")]
        public uint Field4F { get; set; }

        [JsonProperty("hash1")]
        public uint Hash1 { get; set; }

        [JsonProperty("hash2")]
        public uint Hash2 { get; set; }

        [JsonProperty("struct_fields")]
        public required List<StructField> StructFields { get; set; }

        [JsonProperty("enum_fields")]
        public required List<EnumField> EnumFields { get; set; }
    }
}