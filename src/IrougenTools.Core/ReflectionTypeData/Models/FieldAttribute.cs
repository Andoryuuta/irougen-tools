using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
{
    public class FieldAttribute
    {
        [JsonProperty("type")]
        public required TypeClass Type { get; set; }

        [JsonProperty("value")]
        public required string Value { get; set; }
    }
}