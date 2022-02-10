namespace MappingCloningExtensions;
internal static class SymbolExtensions
{
    public static PropertyModel? GetProperty(this IPropertySymbol propertySymbol, TempMapInfo main, BasicList<IPropertySymbol> targetProperties, BasicList<TempCloneInfo> list)
    {
        if (main.Source is null)
        {
            return null;
        }
        if (main.IgnoreCalls is not null && ParseUtils.PropertyHasExpectedValueAlone(main.IgnoreCalls, propertySymbol, main.Source))
        {
            return null;
        }
        IPropertySymbol? target = propertySymbol.GetProperty(targetProperties);
        if (target == null)
        {
            return null;
        }
        PropertyModel output = new();
        output.PropertySymbol = propertySymbol;
        output.ListCategory = propertySymbol.GetListCategory();
        if (main.IsViewModelBase && CanShowIViewModelBaseProperty(output) == false)
        {
            return null;
        }
        EnumListCategory targetCategory;
        targetCategory = target.GetListCategory();
        if (targetCategory != output.ListCategory)
        {
            output.ErrorMessage = $"List types don't match for property {propertySymbol.Name} and the source class was {main.Source.Name} and the target class was {main.Target!.Name}";
            return output;
        }
        if (output.ListCategory == EnumListCategory.None)
        {
            if (propertySymbol.Type.OriginalDefinition.ToDisplayString() != target.Type.OriginalDefinition.ToDisplayString())
            {
                output.ErrorMessage = $"Invalid cast for property {propertySymbol.Name} and the source class was {main.Source.Name} and the target class was {main.Target!.Name}";
                return output;
            }
        }
        if (output.ListCategory != EnumListCategory.Double && main.IsViewModelBase)
        {
            output.Cloneable = false;
            return output;
        }
        bool forceClone = false;
        if (propertySymbol.HasAttribute(aa.ForceClone.ForceCloneAttribute))
        {
            forceClone = true;
        }
        bool preventDeep = false;
        if (main.PreventDeepCalls is not null && ParseUtils.PropertyHasExpectedValueAlone(main.PreventDeepCalls, propertySymbol, main.Source))
        {
            preventDeep = true;
        }
        if (output.ListCategory == EnumListCategory.Single)
        {
            var other1 = propertySymbol.Type.GetSingleGenericTypeUsed();
            var targetSymbol1 = target.Type.GetSingleGenericTypeUsed();
            if (other1!.OriginalDefinition.ToDisplayString() != targetSymbol1!.ToDisplayString())
            {
                return output.PopulateInvalidCastError(propertySymbol, main);
            }
            output.Cloneable = other1.IsCloneable(main.Source, list, preventDeep, forceClone);
        }
        else if (output.ListCategory == EnumListCategory.Double)
        {
            var other1 = propertySymbol.Type.GetSingleGenericTypeUsed();
            var other2 = other1!.GetSingleGenericTypeUsed();
            var targetSymbol1 = target.Type.GetSingleGenericTypeUsed();
            var targetSymbol2 = targetSymbol1!.GetSingleGenericTypeUsed();

            if (other2!.OriginalDefinition.ToDisplayString() != targetSymbol2!.ToDisplayString())
            {
                return output.PopulateInvalidCastError(propertySymbol, main);
            }
            if (main.IsViewModelBase == false)
            {
                output.Cloneable = other2.IsCloneable(main.Source, list, preventDeep, forceClone);
            }

            output.CollectionNameSpace = $"global::{other1!.ContainingSymbol.ToDisplayString()}.{other1.Name}<global::{other2!.ContainingSymbol.ToDisplayString()}.{other2.Name}>";
        }
        else
        {
            output.Cloneable = propertySymbol.Type.IsCloneable(main.Source, list, preventDeep, forceClone);
        }
        return output;
    }
    private static IPropertySymbol? GetProperty(this IPropertySymbol propertySymbol, BasicList<IPropertySymbol> targetList)
    {
        foreach (var item in targetList)
        {
            if (item.Name == propertySymbol.Name)
            {
                return item;
            }
        }
        return null;
    }
    private static PropertyModel PopulateInvalidCastError(this PropertyModel output, IPropertySymbol propertySymbol, TempMapInfo main)
    {
        output.ErrorMessage = $"List types don't match for property {propertySymbol.Name} and the source class was {main.Source!.Name} and the target class was {main.Target!.Name}";
        return output;
    }
    public static PropertyModel? GetProperty(this IPropertySymbol propertySymbol, TempCloneInfo main, BasicList<TempCloneInfo> list)
    {
        if (main.Symbol is null)
        {
            return null;
        }
        if (main.IgnoreCalls is not null && ParseUtils.PropertyHasExpectedValueAlone(main.IgnoreCalls, propertySymbol, main.Symbol))
        {
            return null;
        }
        if (propertySymbol.HasAttribute(aa.IgnoreClone.IgnoreCloneAttribute))
        {
            return null;
        }
        bool forceClone = false;
        if (propertySymbol.HasAttribute(aa.ForceClone.ForceCloneAttribute))
        {
            forceClone = true;
        }
        bool preventDeep = false;
        if (main.Clone is not null)
        {
            (bool rets, string value) = ParseUtils.PropertyGetExtraInfo(main.Clone, propertySymbol, main.Symbol);
            if (main.Explicit)
            {
                if (rets == false)
                {
                    return null;
                }
            }
            bool.TryParse(value, out preventDeep);
        }
        bool temps = propertySymbol.HasAttribute(aa.Clone.CloneAttribute);
        if (main.Explicit)
        {
            if (temps == false)
            {
                return null;
            }
        }
        propertySymbol.TryGetAttribute(aa.Clone.CloneAttribute!, out var attributes);
        if (attributes is not null && attributes.Count() > 0)
        {
            temps = attributes.AttributePropertyValue<bool>(aa.Clone.GetPreventDeepCopyInfo);
            if (temps && preventDeep == false)
            {
                preventDeep = true;
            }
        }
        PropertyModel output = new();
        output.ListCategory = propertySymbol.GetListCategory();
        output.PropertySymbol = propertySymbol;
        if (main.IsViewModelBase)
        {
            if (CanShowIViewModelBaseProperty(output) == false)
            {
                return null;
            }
            if (output.ListCategory != EnumListCategory.Double)
            {
                output.Cloneable = false;
                return output;
            }
        }
        if (output.ListCategory == EnumListCategory.Double)
        {
            var other1 = propertySymbol.Type.GetSingleGenericTypeUsed();
            var other2 = other1!.GetSingleGenericTypeUsed();
            output.CollectionNameSpace = $"global::{other1!.ContainingSymbol.ToDisplayString()}.{other1.Name}<global::{other2!.ContainingSymbol.ToDisplayString()}.{other2.Name}>";
            if (main.IsViewModelBase == false)
            {
                output.Cloneable = other2!.IsCloneable(main.Symbol, list, preventDeep, forceClone);
            }
            else
            {
                output.Cloneable = false;
            }
        }
        else if (output.ListCategory == EnumListCategory.Single)
        {
            var other1 = propertySymbol.Type.GetSingleGenericTypeUsed();
            output.Cloneable = other1!.IsCloneable(main.Symbol, list, preventDeep, forceClone);
        }
        else
        {
            output.Cloneable = propertySymbol.Type.IsCloneable(main.Symbol, list, preventDeep, forceClone);
        }

        return output;
    }
    private static bool IsCloneable(this ITypeSymbol symbol, BasicList<TempCloneInfo> list)
    {
        foreach (var item in list)
        {
            if (item.Symbol!.Name == symbol.Name && item.Symbol.OriginalDefinition.ToDisplayString() == symbol.OriginalDefinition.ToDisplayString()) //probably mistake here.
            {
                return true;
            }
        }
        return false;
    }
    private static bool IsCloneable(this ITypeSymbol propertySymbol, INamedTypeSymbol classSymbol, BasicList<TempCloneInfo> list, bool preventDeep, bool forceClone)
    {
        if (propertySymbol.IsSimpleType())
        {
            return false;
        }
        if (SymbolEqualityComparer.Default.Equals(propertySymbol, classSymbol))
        {
            return false;
        }
        if (propertySymbol.IsRecord)
        {
            return false;
        }
        if (propertySymbol.IsTupleType)
        {
            return false;
        }
        if (propertySymbol.IsValueType)
        {
            return false;
        }
        if (forceClone)
        {
            return true;
        }

        bool rets = propertySymbol.IsCloneable(list);
        if (rets == false)
        {
            return false;
        }
        return preventDeep == false;
    }
    private static EnumListCategory GetListCategory(this IPropertySymbol pp)
    {
        if (pp.IsCollection() == false)
        {
            return EnumListCategory.None;
        }
        var others = pp.GetSingleGenericTypeUsed()!;
        return others.IsCollection() ? EnumListCategory.Double : EnumListCategory.Single;
    }
    private static bool CanShowIViewModelBaseProperty(PropertyModel model)
    {
        if (model.ListCategory != EnumListCategory.None)
        {
            return true;
        }
        if (model.PropertySymbol!.IsSimpleType())
        {
            return true;
        }
        if (model.PropertySymbol!.Type.IsRecord)
        {
            return true;
        }
        if (model.PropertySymbol.Type.IsValueType)
        {
            return true;
        }
        return false;
    }
}