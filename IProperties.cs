namespace MappingCloningExtensions;
internal interface IProperties
{
    BasicList<PropertyModel> Properties { get; set; }
    public bool Collection { get; set; }
    //public INamedTypeSymbol? SymbolUsed { get; set; }
}