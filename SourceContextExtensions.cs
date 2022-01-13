namespace MappingCloningExtensions;
internal static class SourceContextExtensions
{
    public static void RaiseExtraProcessException(this SourceProductionContext context, string information)
    {
        context.ReportDiagnostic(Diagnostic.Create(NoExtraProcess(information), Location.None));
    }
    public static void RaiseCastException(this SourceProductionContext context, string information)
    {
        context.ReportDiagnostic(Diagnostic.Create(InvalidCastError(information), Location.None));
    }
    private static DiagnosticDescriptor InvalidCastError(string information) => new("FourthID",
       "Could not create mappings",
       information,
       "MapID",
       DiagnosticSeverity.Error,
       true
       );
    private static DiagnosticDescriptor NoExtraProcess(string information) => new("Fifth",
       "Could not create post process",
       information,
       "PostProcess",
       DiagnosticSeverity.Error,
       true
       );
}