using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
{
    public class StructField
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("type_name")]
        public required string TypeName { get; set; }

        [JsonProperty("data_offset")]
        public ulong DataOffset { get; set; }

        [JsonProperty("field_attribute")]
        public FieldAttribute? FieldAttribute { get; set; }
    }
}