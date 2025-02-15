using System.Collections.Generic;
using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData.Models
{
    public class NamespaceTree
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("parent")]
        public NamespaceTree? Parent { get; set; }
    }
}