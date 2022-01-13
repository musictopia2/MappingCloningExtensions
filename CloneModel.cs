namespace MappingCloningExtensions;
internal class CloneModel : IProperties
{
    public bool Explicit { get; set; }
    public string NamespaceName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string GetGlobalName => $"global::{NamespaceName}.{ClassName}";
    public bool IsViewModelBase { get; set; }
    public BasicList<PropertyModel> Properties { get; set; } = new();
}