using System.Diagnostics;

namespace MappingCloningExtensions;
[Generator]
public class MySourceGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
//#if DEBUG
//        if (Debugger.IsAttached == false)
//        {
//            Debugger.Launch();
//        }
//#endif
        context.RegisterPostInitializationOutput(c =>
        {
            c.CreateCustomSource().AddAttributesToSourceOnly();
            c.CreateCustomSource().BuildSourceCode();
        });
        IncrementalValuesProvider<NodeInformation> declares = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => IsSyntaxTarget(s),
            (t, _) => GetTarget(t))
            .Where(m => m != null)!;
        IncrementalValueProvider<(Compilation, ImmutableArray<NodeInformation>)> compilation
            = context.CompilationProvider.Combine(declares.Collect());
        context.RegisterSourceOutput(compilation, (spc, source) =>
        {
            Execute(source.Item1, source.Item2, spc);
        });
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        bool rets = syntax is ClassDeclarationSyntax ctx &&
            ctx.BaseList is not null &&
            ctx.ToString().Contains(nameof(MainContext));
        if (rets)
        {
            return true;
        }
        return syntax is ClassDeclarationSyntax cc && cc.AttributeLists.Count > 0;
    }
    private NodeInformation? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode();
        NodeInformation output;
        //if (ourClass.BaseList is not null && ourClass.ToString().Contains(nameof(MainContext)))
        //{
        //    output = new();
        //    output.Node = ourClass;
        //    output.Source = EnumSourceCategory.Fluent;
        //    return output;
        //}
        var symbol = (INamedTypeSymbol) context.SemanticModel.GetDeclaredSymbol(context.Node)!;
        if (symbol.InheritsFrom("MainContext") && symbol.Name != "MainContext") //you should not be calling MainContext.  MainContext would probably be something else in this case.
        {
            output = new();
            output.Node = ourClass;
            output.Source = EnumSourceCategory.Fluent;
            return output;
        }


        bool rets = symbol!.HasAttribute(aa.Cloneable.CloneableAttribute);
        if (rets == false)
        {
            return null;
        }
        output = new();
        output.Node = ourClass;
        output.Source = EnumSourceCategory.Attribute;
        return output;
    }
    private void Execute(Compilation compilation, ImmutableArray<NodeInformation> list, SourceProductionContext context)
    {
        var others = list.Distinct();
        ParserClass parses = new(compilation);
        var completes = parses.GetResults(others);
        EmitClass emit = new(completes, context, compilation);
        emit.Emit();
    }
}