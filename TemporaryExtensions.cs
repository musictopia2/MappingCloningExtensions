namespace MappingCloningExtensions;
internal static class TemporaryExtensions
{
    //for now, decided to not update the helpers.  could decide to do so in the future (not sure if it will include generics or not)
    public static IWriter PopulateFullNamespace(this IWriter w, INamedTypeSymbol symbol)
    {
        //for now, no generics.
        w.GlobalWrite()
            .Write(symbol.ContainingNamespace)
            .Write(".")
            .Write(symbol.Name);
        return w;
    }
}
