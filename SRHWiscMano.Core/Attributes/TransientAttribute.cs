using System.Diagnostics.CodeAnalysis;

namespace SRHWiscMano.Core.Attributes;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : Attribute
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? InterfaceType { get; init; }

    public TransientAttribute() { }

    public TransientAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }
}
