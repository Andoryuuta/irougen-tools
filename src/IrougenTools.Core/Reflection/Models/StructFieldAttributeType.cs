using Newtonsoft.Json;

namespace IrougenTools.Core.Reflection.Models
{
    public class StructFieldAttributeType
    {
        [JsonProperty("namespace")]
        public required NamespaceTree Namespace { get; set; }

        [JsonProperty("desc")]
        public required string Description { get; set; }

        [JsonProperty("referenced_type_name")]
        public string? ReferencedTypeName { get; set; }
    }
}