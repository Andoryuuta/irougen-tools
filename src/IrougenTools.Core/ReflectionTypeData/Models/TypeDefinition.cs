using System.Collections.Generic;
using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
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
        public ulong ClassSize { get; set; }

        [JsonProperty("alignment_1")]
        public ulong Alignment1 { get; set; }

        [JsonProperty("alignment_2")]
        public ulong Alignment2 { get; set; }

        [JsonProperty("fields_count")]
        public ulong FieldsCount { get; set; }

        [JsonProperty("primitive_type")]
        public required string PrimitiveType { get; set; }

        [JsonProperty("field_4D")]
        public ulong Field4D { get; set; }

        [JsonProperty("field_4E")]
        public ulong Field4E { get; set; }

        [JsonProperty("field_4F")]
        public ulong Field4F { get; set; }

        [JsonProperty("hash1")]
        public ulong Hash1 { get; set; }

        [JsonProperty("hash2")]
        public ulong Hash2 { get; set; }

        [JsonProperty("struct_fields")]
        public required List<StructField> StructFields { get; set; }

        [JsonProperty("enum_fields")]
        public required List<EnumField> EnumFields { get; set; }
    }
}