using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Tests.Elements.Expressions;

public class UnaryExpressionTests
{
    private static ConstantExpression IntExpr(int value = 1) => new(value);

    /// <summary>Not → TypeModel.Bool.</summary>
    [Fact]
    public void Constructor_NotOperator_ResultIsBool()
    {
        var expr = new UnaryExpression(UnaryOperator.Not, IntExpr());
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>Negate → operand.ResultType.</summary>
    [Fact]
    public void Constructor_NegateOperator_ResultIsOperandType()
    {
        var operand = IntExpr(42);
        var expr = new UnaryExpression(UnaryOperator.Negate, operand);
        expr.ResultType.Should().Be(operand.ResultType);
    }

    /// <summary>BitwiseNot → operand.ResultType.</summary>
    [Fact]
    public void Constructor_BitwiseNot_ResultIsOperandType()
    {
        var operand = IntExpr(42);
        var expr = new UnaryExpression(UnaryOperator.BitwiseNot, operand);
        expr.ResultType.Should().Be(operand.ResultType);
    }

    /// <summary>Increment → operand.ResultType.</summary>
    [Fact]
    public void Constructor_Increment_ResultIsOperandType()
    {
        var operand = IntExpr(42);
        var expr = new UnaryExpression(UnaryOperator.Increment, operand);
        expr.ResultType.Should().Be(operand.ResultType);
    }

    /// <summary>Decrement → operand.ResultType.</summary>
    [Fact]
    public void Constructor_Decrement_ResultIsOperandType()
    {
        var operand = IntExpr(42);
        var expr = new UnaryExpression(UnaryOperator.Decrement, operand);
        expr.ResultType.Should().Be(operand.ResultType);
    }

    /// <summary>Explicitní resultType má přednost.</summary>
    [Fact]
    public void Constructor_ExplicitResultType_OverridesInferred()
    {
        var expr = new UnaryExpression(UnaryOperator.Negate, IntExpr(), TypeModel.String);
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }
}
