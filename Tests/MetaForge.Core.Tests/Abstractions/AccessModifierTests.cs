using FluentAssertions;
using MetaForge.Core.Abstractions;

namespace MetaForge.Core.Tests.Abstractions;

public class AccessModifierTests
{
    /// <summary>Enum obsahuje všechny očekávané členy.</summary>
    [Fact]
    public void Enum_HasAllExpectedMembers()
    {
        var values = Enum.GetValues<AccessModifier>();
        values.Should().Contain(AccessModifier.Public);
        values.Should().Contain(AccessModifier.Internal);
        values.Should().Contain(AccessModifier.Protected);
        values.Should().Contain(AccessModifier.Private);
        values.Should().Contain(AccessModifier.ProtectedInternal);
        values.Should().Contain(AccessModifier.PrivateProtected);
    }
}
