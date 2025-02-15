using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
{
    public class TypeClass
    {
        [JsonProperty("namespace")]
        public required NamespaceTree Namespace { get; set; }

        [JsonProperty("desc")]
        public required string Description { get; set; }

        [JsonProperty("referenced_type_name")]
        public string? ReferencedTypeName { get; set; }
    }
}