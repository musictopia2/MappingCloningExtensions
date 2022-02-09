namespace MappingCloningExtensions;
internal class CloneModel : IProperties
{
    public bool Explicit { get; set; }
    public INamedTypeSymbol? SymbolUsed { get; set; }
    public string FileName { get; set; } = "";
    public string GlobalName { get; set; } = "";
    //if its a list, then different treatment has to be done.
    //public string NamespaceName { get; set; } = "";
    //public string ClassName { get; set; } = "";
    //public string GetGlobalName => $"global::{NamespaceName}.{ClassName}";
    public bool IsViewModelBase { get; set; }
    public BasicList<PropertyModel> Properties { get; set; } = new();
    public bool Collection { get; set; }
}