using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Tests.Elements.Expressions;

public class BinaryExpressionTests
{
    private static ConstantExpression IntExpr(int value = 1) => new(value);
    private static ConstantExpression DecimalExpr(decimal value = 1m) => new(value);
    private static ConstantExpression StringExpr(string value = "a") => new(value);

    /// <summary>Equal → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_EqualOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Equal, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>NotEqual → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_NotEqualOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.NotEqual, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>GreaterThan → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_GreaterThanOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.GreaterThan, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>LessThan → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_LessThanOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.LessThan, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>GreaterThanOrEqual → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_GreaterThanOrEqualOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.GreaterThanOrEqual, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>LessThanOrEqual → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_LessThanOrEqualOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.LessThanOrEqual, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>Oba operandy Int32 → TypeModel.Int32.</summary>
    [Fact]
    public void Constructor_ArithmeticOperatorBothInt32_ResultIsInt32()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Add, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>Levý operand Decimal → type promotion na Decimal.</summary>
    [Fact]
    public void Constructor_ArithmeticOperatorLeftDecimal_ResultIsDecimal()
    {
        var expr = new BinaryExpression(DecimalExpr(), BinaryOperator.Add, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>Pravý operand Decimal → type promotion na Decimal.</summary>
    [Fact]
    public void Constructor_ArithmeticOperatorRightDecimal_ResultIsDecimal()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Add, DecimalExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>Subtract s Decimal promotion → Decimal.</summary>
    [Fact]
    public void Constructor_SubtractWithDecimal_ResultIsDecimal()
    {
        var expr = new BinaryExpression(DecimalExpr(), BinaryOperator.Subtract, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>Multiply s Decimal promotion → Decimal.</summary>
    [Fact]
    public void Constructor_MultiplyWithDecimal_ResultIsDecimal()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Multiply, DecimalExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>Divide s Decimal promotion → Decimal.</summary>
    [Fact]
    public void Constructor_DivideWithDecimal_ResultIsDecimal()
    {
        var expr = new BinaryExpression(DecimalExpr(), BinaryOperator.Divide, DecimalExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Decimal);
    }

    /// <summary>Concat → TypeModel.String.</summary>
    [Fact]
    public void Constructor_ConcatOperator_ResultIsString()
    {
        var expr = new BinaryExpression(StringExpr(), BinaryOperator.Concat, StringExpr());
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }

    /// <summary>And → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_AndOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.And, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>Or → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_OrOperator_ResultIsBool()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Or, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>NullCoalesce není v switchi → fallback na left.ResultType.</summary>
    [Fact]
    public void Constructor_NullCoalesce_FallsBackToLeftResultType()
    {
        var left = IntExpr(5);
        var expr = new BinaryExpression(left, BinaryOperator.NullCoalesce, IntExpr(10));
        expr.ResultType.BaseType.Should().Be(DataType.Int32);
    }

    /// <summary>Explicitní resultType má přednost.</summary>
    [Fact]
    public void Constructor_ExplicitResultType_OverridesInferred()
    {
        var expr = new BinaryExpression(IntExpr(), BinaryOperator.Add, IntExpr(), TypeModel.String);
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }
}
