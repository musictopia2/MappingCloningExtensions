namespace MappingCloningExtensions;
internal class MapModel : IProperties
{
    public ClassModel? Source { get; set; }
    public ClassModel? Target { get; set; }
    public BasicList<PropertyModel> Properties { get; set; } = new();
    public ArgumentSyntax? PostProcess { get; set; }
    public ArgumentSyntax? Activator { get; set; }
    public string Error { get; set; } = "";
    public bool IsMappable { get; set; } //if mappable, then needs to add to the globals.  but if not mappable, then don't add to global.
    public bool IsViewModelBase { get; set; }
    public bool Collection { get; set; }
}