namespace MappingCloningExtensions;
internal class ClassModel
{
    public string NamespaceName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string GetGlobalName => $"global::{NamespaceName}.{ClassName}";
}