using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Tests.Elements.Expressions;

public class ConstantExpressionTests
{
    /// <summary>null → TypeModel.Object.</summary>
    [Fact]
    public void Constructor_NullValue_InferObject()
    {
        var expr = new ConstantExpression(null);
        expr.ResultType.BaseType.Should().Be(DataType.Object);
        expr.Value.Should().BeNull();
    }

    /// <summary>string → TypeModel.String.</summary>
    [Fact]
    public void Constructor_StringValue_InferString()
    {
        var expr = new ConstantExpression("hello");
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }

    /// <summary>int → TypeModel.Int32.</summary>
    [Fact]
    public void Constructor_IntValue_InferInt32()
    {
        var expr = new ConstantExpression(42);
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>long → TypeModel.Int32.</summary>
    [Fact]
    public void Constructor_LongValue_InferInt32()
    {
        var expr = new ConstantExpression(42L);
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>short → TypeModel.Int32.</summary>
    [Fact]
    public void Constructor_ShortValue_InferInt32()
    {
        var expr = new ConstantExpression((short)42);
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>byte → TypeModel.Int32.</summary>
    [Fact]
    public void Constructor_ByteValue_InferInt32()
    {
        var expr = new ConstantExpression((byte)42);
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>decimal → TypeModel.Decimal.</summary>
    [Fact]
    public void Constructor_DecimalValue_InferDecimal()
    {
        var expr = new ConstantExpression(42.5m);
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>double → TypeModel.Decimal.</summary>
    [Fact]
    public void Constructor_DoubleValue_InferDecimal()
    {
        var expr = new ConstantExpression(42.5);
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>float → TypeModel.Decimal.</summary>
    [Fact]
    public void Constructor_FloatValue_InferDecimal()
    {
        var expr = new ConstantExpression(42.5f);
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>bool → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_BoolValue_InferBool()
    {
        var expr = new ConstantExpression(true);
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>DateTime → TypeModel.DateTime.</summary>
    [Fact]
    public void Constructor_DateTimeValue_InferDateTime()
    {
        var expr = new ConstantExpression(new DateTime(2026, 1, 1));
        expr.ResultType.BaseType.Should().Be(DataType.DateTime);
    }

    /// <summary>DateTimeOffset → TypeModel.DateTime.</summary>
    [Fact]
    public void Constructor_DateTimeOffsetValue_InferDateTime()
    {
        var expr = new ConstantExpression(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        expr.ResultType.BaseType.Should().Be(DataType.DateTime);
    }

    /// <summary>Guid → TypeModel.Guid.</summary>
    [Fact]
    public void Constructor_GuidValue_InferGuid()
    {
        var expr = new ConstantExpression(Guid.NewGuid());
        expr.ResultType.BaseType.Should().Be(DataType.Guid);
    }

    /// <summary>Explicitní resultType má přednost před inferencí.</summary>
    [Fact]
    public void Constructor_ExplicitResultType_OverridesInferred()
    {
        var expr = new ConstantExpression(42, TypeModel.String);
        expr.ResultType.BaseType.Should().Be(DataType.String);
        expr.Value.Should().Be(42);
    }

    /// <summary>uint není v pattern match → TypeModel.Object (edge case).</summary>
    [Fact]
    public void Constructor_UInt_FallsBackToObject()
    {
        var expr = new ConstantExpression(42u);
        expr.ResultType.BaseType.Should().Be(DataType.Object);
    }

    /// <summary>BigInteger není v pattern match → TypeModel.Object (edge case).</summary>
    [Fact]
    public void Constructor_BigInteger_FallsBackToObject()
    {
        var expr = new ConstantExpression(System.Numerics.BigInteger.One);
        expr.ResultType.BaseType.Should().Be(DataType.Object);
    }

    /// <summary>char není v pattern match → TypeModel.Object (edge case).</summary>
    [Fact]
    public void Constructor_Char_FallsBackToObject()
    {
        var expr = new ConstantExpression('x');
        expr.ResultType.BaseType.Should().Be(DataType.Object);
    }
}
