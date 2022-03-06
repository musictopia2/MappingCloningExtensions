namespace MappingCloningExtensions;
internal static class WriterExtensions
{
    public static void PopulateMapToMethod(this ICodeBlock w, MapModel result, Action<ICodeBlock> action)
    {
        w.WriteLine(w =>
        {
            w.Write("public static ")
            .PopulateFullNamespace(result.Target!)
            .Write(" MapTo(this ")
            .PopulateFullNamespace(result.Source!)
            .Write(" source)");
        }).WriteCodeBlock(w =>
        {
            action.Invoke(w);
        });
    }
    public static void PopulateMapToSafeMethod(this ICodeBlock w, MapModel result, Action<ICodeBlock> action)
    {
        w.WriteLine(w =>
        {
            w.Write("public static ")
            .PopulateFullNamespace(result.Target!)
            .Write(" MapToSafe(this ")
            .PopulateFullNamespace(result.Source!)
            .Write(" source, global::System.Collections.Generic.Stack<object>? referenceChain = null)");
        }).WriteCodeBlock(w =>
        {
            action.Invoke(w);
        });
    }
    public static void PopulateCloneMethod(this ICodeBlock w, CloneModel result, Action<ICodeBlock> action)
    {
        w.WriteLine("/// <summary>")
            .WriteLine("/// Creates a copy of SafeDeepCloneChild with NO circular reference checking. This method should be used if performance matters.")
            .WriteLine(w =>
            {
                w.Write("/// <exception cref=")
                .AppendDoubleQuote("StackOverflowException")
                .Write(">Will occur on any object that has circular references in the hierarchy.</exception>");
            })
            .WriteLine("/// </summary>")
            .PopulateSource()
            .WriteLine(w =>
            {
                w.Write("public static ")
                .Write(result.GlobalName)
                .Write(" Clone(this ")
                .Write(result.GlobalName)
                .Write(" source)");
            }).WriteCodeBlock(w =>
            {
                action.Invoke(w);
            });
    }

    private static ICodeBlock PopulateSource(this ICodeBlock w)
    {
        w.WriteLine(w =>
        {
            w.Write("/// <param name=")
            .AppendDoubleQuote("source")
            .Write(">Extension method</param>");
        });
        return w;
    }
    public static void PopulateCloneSafeMethod(this ICodeBlock w, CloneModel result, Action<ICodeBlock> action)
    {
        w.WriteLine("/// <summary>")
           .WriteLine("/// Creates a copy of SafeDeepCloneChild with circular reference checking. If a circular reference was detected, only a reference of the leaf object is passed instead of cloning it.")
           .WriteLine(w =>
           {
               w.Write("/// <exception cref=")
               .AppendDoubleQuote("StackOverflowException")
               .Write(">Will occur on any object that has circular references in the hierarchy.</exception>");
           })
           .WriteLine("/// </summary>")
           .PopulateSource()
           .WriteLine(w =>
           {
               w.Write("/// <param name=")
               .AppendDoubleQuote("referenceChain")
               .Write(">Should only be provided if specific objects should not be cloned but passed by reference instead.</param>");
           })
           .WriteLine(w =>
           {
               w.Write("public static ")
               .Write(result.GlobalName)
               .Write(" CloneSafe(this ")
               .Write(result.GlobalName)
               .Write(" source, global::System.Collections.Generic.Stack<object>? referenceChain = null)");
           }).WriteCodeBlock(w =>
           {
               action.Invoke(w);
           });
    }
}