using System.Collections.Generic;
using System.IO;
using IrougenTools.Core.ReflectionTypeData.Models;
using Newtonsoft.Json;

namespace IrougenTools.Core.ReflectionTypeData
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