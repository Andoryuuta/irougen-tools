using System.Collections.Generic;
using System.IO;
using IrougenTools.Core.Reflection.Models;
using Newtonsoft.Json;

namespace IrougenTools.Core.Reflection
{
    public static class TypeDefinitionLoader
    {
        public static List<TypeDefinition> LoadFromFile(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<TypeDefinition>>(jsonContent) 
                   ?? throw new JsonSerializationException("Failed to deserialize TypeDefinition list");
        }

        public static List<TypeDefinition> LoadFromJson(string jsonContent)
        {
            return JsonConvert.DeserializeObject<List<TypeDefinition>>(jsonContent)
                   ?? throw new JsonSerializationException("Failed to deserialize TypeDefinition list");
        }
    }
}