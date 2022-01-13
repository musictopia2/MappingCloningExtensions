global using MappingCloningExtensions;
namespace MappingCloningExtensions;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
internal sealed class CloneableAttribute : Attribute
{
    [Required]
    public bool ExplicitDeclaration { get; set; }
}