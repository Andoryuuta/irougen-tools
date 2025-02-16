using Newtonsoft.Json;

namespace IrougenTools.Core.Reflection.Models
{
    public class StructFieldAttribute
    {
        [JsonProperty("type")]
        public required StructFieldAttributeType Type { get; set; }

        [JsonProperty("value")]
        public required string Value { get; set; }
    }
}