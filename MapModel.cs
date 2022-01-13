namespace MappingCloningExtensions;
internal class MapModel : IProperties
{
    public ClassModel? Source { get; set; }
    public ClassModel? Target { get; set; }
    public BasicList<PropertyModel> Properties { get; set; } = new();
    public ArgumentSyntax? PostProcess { get; set; }
    public ArgumentSyntax? Activator { get; set; }
    public string Error { get; set; } = "";
    public bool IsViewModelBase { get; set; }
}