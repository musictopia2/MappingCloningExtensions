namespace MappingCloningExtensions;
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
internal sealed class CloneAttribute : Attribute
{
    [Required]
    public bool PreventDeepCopy { get; set; }
}