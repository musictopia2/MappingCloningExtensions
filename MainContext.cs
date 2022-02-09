namespace MappingCloningExtensions;

[IncludeCode]
internal interface ICloneConfig<T>
{
    ICloneConfig<T> Clone<P>(bool preventDeep, Func<T, P> propertySelector);
    ICloneConfig<T> Ignore<P>(Func<T, P> propertySelector);
}
internal interface IMapConfig<TSource, TTarget>
{
    IMapConfig<TSource, TTarget> Ignore<P>(Func<TSource, P> propertySelector);
    IMapConfig<TSource, TTarget> PreventDeep<P>(Func<TSource, P> propertySelector);
    IMapConfig<TSource, TTarget> PostProcess(PostProcessDelegate<TTarget, TSource> postProcess);
    IMapConfig<TSource, TTarget> Activator(Func<TSource, TTarget> activator);
}
internal delegate void PostProcessDelegate<TTarget, in TSource>(ref TTarget target, TSource source);
internal interface IMakeConfig<T>
{
    IMakeConfig<T> MapTo<Target>(Action<IMapConfig<T, Target>> config);
    IMakeConfig<T> Cloneable(bool explicitDeclaration, Action<ICloneConfig<T>>? config = null);
}
internal interface ICustomConfig
{
    ICustomConfig Make<T>(Action<IMakeConfig<T>> config);
}
internal abstract class MainContext
{
    public const string ConfigureName = nameof(Configure);
    protected abstract void Configure(ICustomConfig config);
}