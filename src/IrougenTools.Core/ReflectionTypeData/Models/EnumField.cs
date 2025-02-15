using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
{
    public class EnumField
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("value")]
        public ulong Value { get; set; }
    }
}