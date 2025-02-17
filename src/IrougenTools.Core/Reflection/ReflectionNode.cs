using System.Xml.Serialization;
using IrougenTools.Core.Reflection.Models;

namespace IrougenTools.Core.Reflection;

public class ReflectionNode
{
    [XmlAttribute]
    public string? Name { get; set; }
    
    [XmlAttribute]
    public string? Type { get; set; }
    
    public List<ReflectionNode> Children = [];
    
    public object? PrimitiveValue { get; set; }
}