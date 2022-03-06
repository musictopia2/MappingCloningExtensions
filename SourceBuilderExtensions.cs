namespace MappingCloningExtensions;
internal static class SourceBuilderExtensions
{
    public static void WriteMapExtension(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, MapModel result)
    {
        builder.WriteLine("#nullable enable");
        //don't do any usings.  instead, go ahead and do the fully quantified namespaces always.
        builder.WriteLine("namespace CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;")
        .WriteLine("public static partial class ModelExtensions")
        .WriteCodeBlock(w =>
        {
            action.Invoke(w);
        });
    }
    public static void WriteCloneExtension(this SourceCodeStringBuilder builder, Action<ICodeBlock> action)
    {
        builder.WriteLine("#nullable enable")
            .WriteLine("namespace CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;")
            .WriteLine("public static partial class ModelExtensions")
            .WriteCodeBlock(w =>
            {
                action.Invoke(w);
            });
    }
}