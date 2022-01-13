namespace MappingCloningExtensions;
internal static class SourceBuilderExtensions
{
    public static void WriteMapExtension(this SourceCodeStringBuilder builder, Action<ICodeBlock> action, MapModel result)
    {
        builder.WriteLine("#nullable enable")
            .WriteLine(w =>
            {
                w.Write("using ")
                .Write(result.Source!.NamespaceName)
                .Write(";");
            });
        if (result.Target!.NamespaceName != result.Source!.NamespaceName)
        {
            builder.WriteLine(w =>
            {
                w.Write("using ")
                .Write(result.Target!.NamespaceName)
                .Write(";");
            });
        }
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