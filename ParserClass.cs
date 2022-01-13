﻿namespace MappingCloningExtensions;
internal class ParserClass
{
    private readonly Compilation _compilation;
    public ParserClass(Compilation compilation)
    {
        _compilation = compilation;
    }
    private BasicList<TempCloneInfo> _tempClones = new();
    private BasicList<TempMapInfo> _tempMaps = new();
    public CompleteInformation GetResults(IEnumerable<NodeInformation> firsts)
    {
        CompleteInformation output = new();
        _tempMaps = new();
        _tempClones = new();
        foreach (var item in firsts)
        {
            PopulateCompleteList(item);
        }
        foreach (var item in _tempClones)
        {
            CloneModel result = new();
            result.ClassName = item.Symbol!.Name;
            result.Explicit = item.Explicit;
            result.IsViewModelBase = item.IsViewModelBase;
            result.NamespaceName = item.Symbol.ContainingNamespace.ToDisplayString();
            var pList = item.Symbol!.GetAllPublicProperties();
            foreach (var p in pList)
            {
                var fins = p.GetProperty(item, _tempClones);
                if (fins is not null)
                {
                    result.Properties.Add(fins);
                }
            }
            output.Clones.Add(result);
        }
        foreach (var item in _tempMaps)
        {
            MapModel fins = new();
            ClassModel info = new();
            info.ClassName = item.Source!.Name;
            info.NamespaceName = item.Source.ContainingNamespace.ToDisplayString();
            fins.Source = info;
            info = new();
            info.ClassName = item.Target!.Name;
            info.NamespaceName = item.Source.ContainingNamespace.ToDisplayString();
            fins.Target = info;
            fins.Error = item.Error;
            fins.PostProcess = item.PostProcess;
            fins.Activator = item.Activator;
            var pList = item.Source!.GetAllPublicProperties();
            var seconds = item.Target.GetAllPublicProperties();
            fins.IsViewModelBase = item.IsViewModelBase;
            foreach (var p in pList)
            {
                var aa = p.GetProperty(item, seconds, _tempClones);
                if (aa is not null)
                {
                    fins.Properties.Add(aa);
                }
            }
            output.Maps.Add(fins);
        }
        return output;
    }
    private void PopulateCompleteList(NodeInformation item)
    {
        if (item.Source == EnumSourceCategory.Fluent)
        {
            PopulateFluent(item.Node!);
            return;
        }
        PopulateAttribute(item.Node!);
    }
    private void PopulateFluent(ClassDeclarationSyntax node)
    {
        ParseContext context = new(_compilation, node);
        var members = node.DescendantNodes().OfType<MethodDeclarationSyntax>();
        foreach (var member in members)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(member) as IMethodSymbol;
            if (symbol is not null && symbol.Name == MainContext.ConfigureName)
            {
                ParseContext(context, member, symbol);
            }
        }
    }
    private void ParseContext(ParseContext context, MethodDeclarationSyntax syntax, IMethodSymbol symbol)
    {
        var makeCalls = ParseUtils.FindCallsOfMethodWithName(context, syntax, nameof(ICustomConfig.Make));
        foreach (var make in makeCalls)
        {
            INamedTypeSymbol makeType = (INamedTypeSymbol)make.MethodSymbol.TypeArguments[0]!;
            string name = nameof(IMakeConfig<object>.Cloneable);
            var firsts = ParseUtils.FindCallsOfMethodInConfigLambda(context, make, name);
            if (firsts.Count == 1)
            {
                string value = ParseUtils.GetStringContent(firsts);
                bool.TryParse(value, out bool rets);
                var seconds = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(ICloneConfig<object>.Ignore), optional: true, argumentIndex: 1);
                var thirds = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(ICloneConfig<object>.Clone), optional: true, argumentIndex: 1);
                TempCloneInfo? info = GetSavedClone(makeType);
                PopulateTemporaryClone(info, makeType, rets, seconds, thirds);
            }
            name = nameof(IMakeConfig<object>.MapTo);
            firsts = ParseUtils.FindCallsOfMethodInConfigLambda(context, make, name);
            if (firsts.Count == 1)
            {
                string value = ParseUtils.GetStringContent(firsts);
                bool.TryParse(value, out bool rets);
                var seconds = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(IMapConfig<object, object>.Ignore), optional: true); //i think
                var thirds = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(IMapConfig<object, object>.PreventDeep), optional: true);
                TempMapInfo? info = GetSavedMap(makeType);
                if (info is null)
                {
                    info = new();
                    info.IgnoreCalls = seconds;
                    info.PreventDeepCalls = thirds;
                    info.Source = makeType;
                    info.Target = (INamedTypeSymbol)firsts.Single().MethodSymbol.TypeArguments[0];
                    info.IsViewModelBase = info.Source.Implements("IViewModelBase");
                    _tempMaps.Add(info);
                }
                var postProcessCalls = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(IMapConfig<object, object>.PostProcess), optional: true);
                if (postProcessCalls.Count == 1)
                {
                    var arg = postProcessCalls.Single().ArgumentList;
                    if (arg == null || arg.Arguments.Count <= 0)
                    {
                        info.Error = $"Could not find post process funct.  The class used was {makeType.Name}";
                    }
                    else
                    {
                        info.PostProcess = arg.Arguments[0];
                    }
                }
                var activateCalls = ParseUtils.FindCallsOfMethodInConfigLambda(context, firsts.Single(), nameof(IMapConfig<object, object>.Activator), optional: true);
                if (activateCalls.Count == 1)
                {
                    var arg = activateCalls.Single().ArgumentList;
                    if (arg == null || arg.Arguments.Count <= 0)
                    {
                        info.Error = $"Could not find post process funct.  The class used was {makeType.Name}";
                    }
                    else
                    {
                        info.Activator = arg.Arguments[0];
                    }
                }
            }
        }
    }
    private TempMapInfo? GetSavedMap(INamedTypeSymbol symbolLookUp)
    {
        foreach (var item in _tempMaps)
        {
            if (item.Source!.Name == item.Source.Name && item.Source.OriginalDefinition.ToDisplayString() == symbolLookUp.ToDisplayString())
            {
                return item;
            }
        }
        return null;
    }
    private void PopulateAttribute(ClassDeclarationSyntax node)
    {
        SemanticModel compilationSemanticModel = node.GetSemanticModel(_compilation);
        INamedTypeSymbol symbol = (INamedTypeSymbol)compilationSemanticModel.GetDeclaredSymbol(node)!;
        TempCloneInfo? info = GetSavedClone(symbol);
        symbol.TryGetAttribute(aa.Cloneable.CloneableAttribute, out var attributes);
        bool rets = attributes.AttributePropertyValue<bool>(aa.Cloneable.GetExplicitDeclarationInfo);
        PopulateTemporaryClone(info, symbol, rets);
    }
    private TempCloneInfo? GetSavedClone(INamedTypeSymbol symbolLookUp)
    {
        foreach (var item in _tempClones)
        {
            if (item.Symbol!.Name == item.Symbol.Name && item.Symbol.OriginalDefinition.ToDisplayString() == symbolLookUp.ToDisplayString())
            {
                return item;
            }
        }
        return null;
    }
    private void PopulateTemporaryClone(TempCloneInfo? info, INamedTypeSymbol symbol, bool declarationExplicit, IReadOnlyList<CallInfo>? ignore = null, IReadOnlyList<CallInfo>? clone = null)
    {
        if (info is null)
        {
            info = new();
            info.IgnoreCalls = ignore;
            info.Clone = clone;
            info.Explicit = declarationExplicit;
            info.Symbol = symbol;
            info.IsViewModelBase = symbol.Implements("IViewModelBase");
            _tempClones.Add(info);
        }
        else
        {
            if (info.IgnoreCalls is null && ignore is not null)
            {
                info.IgnoreCalls = ignore;
            }
            if (info.Clone is null && clone is not null)
            {
                info.Clone = clone;
            }
            if (info.Explicit == false && declarationExplicit)
            {
                info.Explicit = true;
            }
        }
    }
    private void PopulateLists(ICodeBlock w, IProperties result, EnumMethodCategory method)
    {
        foreach (var p in result.Properties)
        {
            if (p.ListCategory == EnumListCategory.Single)
            {
                PrintSingleListInfo(w, p, method);
            }
            else if (p.ListCategory == EnumListCategory.Double)
            {
                PrintDoubleListInfo(w, p, method);
            }
        }
    }
    private void PrintSingleListInfo(ICodeBlock w, PropertyModel p, EnumMethodCategory method)
    {
        w.WriteLine(w =>
        {
            w.Write("output.")
            .Write(p.Name)
            .Write(" = new ();");
        })
        .WriteLine(w =>
        {
            w.Write("foreach (var item in source.")
            .Write(p.Name)
            .Write(".AsSpan())");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.Write("output.")
                .Write(p.Name)
                .Write(".Add(item");
                if (p.Cloneable == false)
                {
                    w.Write(");");
                }
                else if (method == EnumMethodCategory.Regular)
                {
                    w.Write("?.Clone()!);");
                }
                else
                {
                    w.Write("?.CloneSafe(referenceChain)!);");
                }
            });
        });
    }
    private void PrintDoubleListInfo(ICodeBlock w, PropertyModel p, EnumMethodCategory method)
    {
        w.WriteLine(w =>
        {
            w.Write("output.")
            .Write(p.Name)
            .Write(" = new ();");
        })
        .WriteLine(w =>
        {
            w.Write("foreach (var firsts in source.")
            .Write(p.Name)
            .Write(".AsSpan())");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.Write(p.CollectionNameSpace)
                .Write(" fins = new();");
            })
            .WriteLine("foreach (var seconds in firsts.AsSpan())")
            .WriteCodeBlock(w =>
            {
                w.WriteLine(w =>
                {
                    w.Write("fins.Add(seconds");
                    if (p.Cloneable == false)
                    {
                        w.Write(");");
                    }
                    else if (method == EnumMethodCategory.Regular)
                    {
                        w.Write("?.Clone()!);");
                    }
                    else
                    {
                        w.Write("?.CloneSafe(referenceChain)!);");
                    }
                });
            })
            .WriteLine(w =>
            {
                w.Write("output.")
                .Write(p.Name)
                .Write(".Add(fins);");
            });
        });
    }
}