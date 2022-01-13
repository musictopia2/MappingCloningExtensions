namespace MappingCloningExtensions;
internal class TempCloneInfo
{
    public INamedTypeSymbol? Symbol { get; set; }
    public IReadOnlyList<CallInfo>? IgnoreCalls { get; set; }
    public IReadOnlyList<CallInfo>? Clone { get; set; }
    public bool Explicit { get; set; }
    public bool IsViewModelBase { get; set; }
}