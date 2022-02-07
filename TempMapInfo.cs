namespace MappingCloningExtensions;
internal class TempMapInfo
{
    public INamedTypeSymbol? Source { get; set; }
    public INamedTypeSymbol? Target { get; set; }
    public IReadOnlyList<CallInfo>? IgnoreCalls { get; set; }
    public IReadOnlyList<CallInfo>? PreventDeepCalls { get; set; }
    public ArgumentSyntax? PostProcess { get; set; }
    public ArgumentSyntax? Activator { get; set; }
    public string Error { get; set; } = "";
    public bool IsMappable { get; set; }
    public bool IsViewModelBase { get; set; }
}