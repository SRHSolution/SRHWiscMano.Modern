using System.Diagnostics.CodeAnalysis;

namespace SRHWiscMano.Core.Attributes;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : Attribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? InterfaceType { get; init; }

    public SingletonAttribute() { }

    public SingletonAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }
}
