using Newtonsoft.Json;

namespace IrougenTools.Core.Reflection.Models
{
    public class EnumField
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("value")]
        public ulong Value { get; set; }
    }
}