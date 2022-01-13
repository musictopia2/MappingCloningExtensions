﻿namespace MappingCloningExtensions;
internal class EmitClass
{
    private readonly CompleteInformation _complete;
    private readonly SourceProductionContext _context;
    private readonly Compilation _compilation;
    public EmitClass(CompleteInformation complete, SourceProductionContext context, Compilation compilation)
    {
        _complete = complete;
        _context = context;
        _compilation = compilation;
    }
    private BasicList<PropertyModel> GetPropertyErrors()
    {
        BasicList<PropertyModel> output = new();
        foreach (var item in _complete.Maps)
        {
            foreach (var p in item.Properties)
            {
                if (p.ErrorMessage != "")
                {
                    output.Add(p);
                }
            }
        }
        return output;
    }
    private BasicList<MapModel> GetPostProcessErrors()
    {
        BasicList<MapModel> output = new();
        foreach (var item in _complete.Maps)
        {
            if (item.Error != "")
            {
                output.Add(item);
            }
        }
        return output;
    }
    private void AddMapGlobal()
    {
        if (_complete.Maps.Count == 0)
        {
            return;
        }
        string ns = _compilation.AssemblyName!;
        SourceCodeStringBuilder builder = new();
        builder.WriteLine("#nullable enable")
            .WriteLine("using CommonBasicLibraries.AdvancedGeneralFunctionsAndProcesses.BasicExtensions;")
             .WriteLine(w =>
             {
                 w.Write("namespace ")
                 .Write(ns)
                 .Write(".RegisterMappings;");
             })
             .WriteLine("public static class Registrations")
             .WriteCodeBlock(w =>
             {
                 w.WriteLine("public static void RegisterMappings()")
                 .WriteCodeBlock(w =>
                 {
                     foreach (var item in _complete.Maps)
                     {
                         w.WriteLine(w =>
                         {
                             w.Write("CommonBasicLibraries.BasicDataSettingsAndProcesses.MapHelpers<")
                             .Write(item.Target!.GetGlobalName)
                             .Write(">.AddMap(typeof(")
                             .Write(item.Source!.GetGlobalName)
                             .Write("), item =>");
                         })
                         .WriteLambaBlock(w =>
                         {
                             w.WriteLine(w =>
                             {
                                 w.Write("var fins = item as ")
                                 .Write(item.Source!.GetGlobalName)
                                 .Write(";");
                             })
                             .WriteLine("return fins!.MapTo();");
                         });
                     }
                 });
             });
        _context.AddSource("generatedglobalmappings.g", builder.ToString());
    }
    public void Emit()
    {
        var firsts = GetPropertyErrors();
        bool hadErrors = false;
        if (firsts.Count != 0)
        {
            foreach (var item in firsts)
            {
                _context.RaiseCastException(item.ErrorMessage);
            }
            hadErrors = true;
        }
        var seconds = GetPostProcessErrors();
        if (seconds.Count != 0)
        {
            foreach (var item in seconds)
            {
                _context.RaiseExtraProcessException(item.Error);
            }
            hadErrors = true;
        }
        if (hadErrors)
        {
            return;
        }
        foreach (var item in _complete.Clones)
        {
            ProcessSingleClone(item);
        }
        foreach (var item in _complete.Maps)
        {
            ProcessSingleMap(item);
        }
        AddMapGlobal();
    }
    private void ProcessSingleMap(MapModel result)
    {
        SourceCodeStringBuilder builder = new();
        builder.WriteMapExtension(w =>
        {
            w.PopulateMapToMethod(result, w =>
            {
                ProcessMapToRegular(w, result);
            });
            if (result.IsViewModelBase == false)
            {
                w.PopulateMapToSafeMethod(result, w =>
                {
                    ProcessMapToSafe(w, result);
                });
            }
        }, result);
        _context.AddSource($"{result.Source!.ClassName}.MapExtensions.g", builder.ToString());
    }
    private void ProcessMapToSafe(ICodeBlock w, MapModel result)
    {
        w.WriteLine("referenceChain ??= new ();");
        if (result.Activator is ArgumentSyntax activatorArg)
        {
            CreateActivatorInfo(w, result, activatorArg);
            InitPropertiesLater(w, result, EnumMethodCategory.Safe);
        }
        else
        {
            CreateBasicOutput(w, result, result.Target!.GetGlobalName, EnumMethodCategory.Safe);
        }
        PopulateLists(w, result, EnumMethodCategory.Safe);
        WritePostProcessing(w, result);
        w.WriteLine("referenceChain.Pop();");
        FinalWrite(w);
    }
    private void ProcessMapToRegular(ICodeBlock w, MapModel result)
    {
        if (result.Activator is ArgumentSyntax activatorArg)
        {
            CreateActivatorInfo(w, result, activatorArg);
            InitPropertiesLater(w, result, EnumMethodCategory.Regular);
        }
        else
        {
            CreateBasicOutput(w, result, result.Target!.GetGlobalName, EnumMethodCategory.Regular);
        }
        PopulateLists(w, result, EnumMethodCategory.Regular);
        WritePostProcessing(w, result);
        FinalWrite(w);
    }
    private void WritePostProcessing(ICodeBlock w, MapModel result)
    {
        if (result.PostProcess is null)
        {
            return;
        }
        if (result.PostProcess.ChildNodes().FirstOrDefault() is LambdaExpressionSyntax lambda)
        {
            w.WriteLine(w =>
            {
                w.Write("var postProcess = ")
                .Write(lambda)
                .Write(";");
            })
            .WriteLine("postProcess(ref output, source);");
        }
        else
        {
            w.WriteLine(w =>
            {
                w.Write(result.PostProcess)
                .Write("(ref output, source)");
            });
        }
    }
    private void InitPropertiesLater(ICodeBlock w, MapModel result, EnumMethodCategory method)
    {
        foreach (var p in result.Properties)
        {
            if (p.ListCategory == EnumListCategory.None)
            {
                w.WriteLine(w =>
                {
                    w.Write("output.")
                    .Write(p.Name)
                    .Write(" = ")
                    .Write("source.")
                    .Write(p.Name);
                    if (p.Cloneable)
                    {
                        if (method == EnumMethodCategory.Safe)
                        {
                            w.Write("?.CloneSafe(referenceChain)!");
                        }
                        else
                        {
                            w.Write("?.Clone()!");
                        }
                    }
                    w.Write(";");
                });
            }
        }
    }
    private void CreateActivatorInfo(ICodeBlock w, MapModel result, ArgumentSyntax activatorArg)
    {
        if (activatorArg.ChildNodes().FirstOrDefault() is LambdaExpressionSyntax lambda)
        {
            w.WriteLine(w =>
            {
                w.Write("Func<")
                .Write(result.Source!.ClassName)
                .Write(", ")
                .Write(result.Target!.ClassName)
                .Write("> instanceCreator = ")
                .Write(lambda)
                .Write(";");
            });
            w.WriteLine("var output = instanceCreator(source);");
        }
        else
        {
            w.WriteLine(w =>
            {
                w.Write("var output = ")
                .Write(activatorArg)
                .Write("(source);");
            });
        }
    }
    private void ProcessSingleClone(CloneModel result)
    {
        SourceCodeStringBuilder builder = new();
        builder.WriteCloneExtension(w =>
        {
            w.PopulateCloneMethod(result, w =>
            {
                ProcessCloneRegular(w, result);
            });
            if (result.IsViewModelBase == false)
            {
                w.PopulateCloneSafeMethod(result, w =>
                {
                    ProcessCloneSafe(w, result);
                });
            }
        });
        _context.AddSource($"{result.ClassName}.CloneExtensions.g", builder.ToString());
    }
    private void ProcessCloneRegular(ICodeBlock w, CloneModel result)
    {
        CreateBasicOutput(w, result, result.GetGlobalName, EnumMethodCategory.Regular);
        PopulateLists(w, result, EnumMethodCategory.Regular);
        FinalWrite(w);
    }
    private void ProcessCloneSafe(ICodeBlock w, CloneModel result)
    {
        w.WriteLine("if (referenceChain?.Contains(source) == true)")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("return source;");
            })
            .WriteLine("referenceChain ??= new ();")
            .WriteLine("referenceChain.Push(source);");
        CreateBasicOutput(w, result, result.GetGlobalName, EnumMethodCategory.Safe);
        PopulateLists(w, result, EnumMethodCategory.Safe);
        w.WriteLine("referenceChain.Pop();");
        FinalWrite(w);
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
    private void CreateBasicOutput(ICodeBlock w, IProperties result, string globalName, EnumMethodCategory method)
    {
        void ConstructorProperties(ICodeBlock w)
        {
            BasicList<PropertyModel> list = new();
            foreach (var item in result.Properties)
            {
                if (item.ListCategory == EnumListCategory.None)
                {
                    list.Add(item);
                }
            }
            w.InitializeFromCustomList(list, (w, item) =>
            {
                w.Write(item.Name)
                .Write(" = ")
                .Write("source.")
                .Write(item.Name);
                if (item.Cloneable)
                {
                    if (method == EnumMethodCategory.Safe)
                    {
                        w.Write("?.CloneSafe(referenceChain)!");
                    }
                    else
                    {
                        w.Write("?.Clone()!");
                    }
                }
            });
        }
        w.WriteLine(w =>
        {
            w.Write(globalName)
            .Write(" output = new()");
        }).WriteCodeBlock(w =>
        {
            ConstructorProperties(w);
        }, endSemi: true);
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
    private void FinalWrite(ICodeBlock w)
    {
        w.WriteLine("return output;");
    }
}