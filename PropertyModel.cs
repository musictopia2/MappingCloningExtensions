namespace MappingCloningExtensions;
internal class PropertyModel
{
    public bool Cloneable { get; set; }
    public EnumListCategory ListCategory { get; set; }
    public IPropertySymbol? PropertySymbol { get; set; }
    public string Name => PropertySymbol!.Name;
    public string CollectionNameSpace { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
}