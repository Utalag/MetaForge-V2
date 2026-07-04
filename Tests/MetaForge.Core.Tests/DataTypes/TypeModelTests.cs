using FluentAssertions;
using MetaForge.Core.DataTypes;

namespace MetaForge.Core.Tests.DataTypes;

public class TypeModelTests
{
    [Fact]
    public void String_StaticProperty_HasCorrectBaseType()
    {
        TypeModel.String.BaseType.Should().Be(DataType.String);
        TypeModel.String.IsNullable.Should().BeFalse();
        TypeModel.String.IsCollection.Should().BeFalse();
    }

    [Fact]
    public void Int32_StaticProperty_HasCorrectBaseType()
    {
        TypeModel.Int32.BaseType.Should().Be(DataType.Int32);
    }

    [Fact]
    public void Void_StaticProperty_IsVoid()
    {
        TypeModel.Void.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void MakeNullable_SetsIsNullableTrue()
    {
        var type = TypeModel.String.MakeNullable();
        type.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void MakeCollection_SetsIsCollectionTrue()
    {
        var type = TypeModel.String.MakeCollection();
        type.IsCollection.Should().BeTrue();
    }

    [Fact]
    public void WithCustomName_SetsCustomTypeName()
    {
        var type = TypeModel.Of(DataType.Entity).WithCustomName("Customer");
        type.CustomTypeName.Should().Be("Customer");
    }

    [Fact]
    public void WithGenericArg_AddsArgument()
    {
        var type = TypeModel.Of(DataType.Array).WithGenericArg(TypeModel.String);
        type.GenericArguments.Should().HaveCount(1);
        type.GenericArguments[0].BaseType.Should().Be(DataType.String);
    }

    [Fact]
    public void Of_CreatesTypeWithGivenBaseType()
    {
        var type = TypeModel.Of(DataType.Guid);
        type.BaseType.Should().Be(DataType.Guid);
    }

    [Fact]
    public void Immutability_MakeNullable_DoesNotModifyOriginal()
    {
        var original = TypeModel.String;
        var nullable = original.MakeNullable();
        original.IsNullable.Should().BeFalse();
        nullable.IsNullable.Should().BeTrue();
    }
}
