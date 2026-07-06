using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Tests.Elements.Expressions;

public class ExpressionBasicTests
{
    /// <summary>ExpressionKind enum obsahuje všechny očekávané členy.</summary>
    [Fact]
    public void ExpressionKind_HasExpectedMembers()
    {
        var values = Enum.GetValues<ExpressionKind>();
        values.Should().Contain(ExpressionKind.Constant);
        values.Should().Contain(ExpressionKind.MemberAccess);
        values.Should().Contain(ExpressionKind.Binary);
        values.Should().Contain(ExpressionKind.Unary);
        values.Should().Contain(ExpressionKind.MethodCall);
        values.Should().Contain(ExpressionKind.Lambda);
        values.Should().Contain(ExpressionKind.New);
        values.Should().Contain(ExpressionKind.Conditional);
        values.Should().Contain(ExpressionKind.Computed);
    }

    /// <summary>MethodName a Arguments se nastaví.</summary>
    [Fact]
    public void MethodCall_Constructor_SetsProperties()
    {
        var args = new Expression[] { new ConstantExpression("test") };
        var expr = new MethodCallExpression("string.IsNullOrEmpty", args, TypeModel.Bool);

        expr.MethodName.Should().Be("string.IsNullOrEmpty");
        expr.Arguments.Should().HaveCount(1);
        expr.ResultType.BaseType.Should().Be(DataType.Bool);
    }

    /// <summary>Prázdné argumenty.</summary>
    [Fact]
    public void MethodCall_NoArguments_EmptyList()
    {
        var expr = new MethodCallExpression("System.Guid.NewGuid", Array.Empty<Expression>());
        expr.Arguments.Should().BeEmpty();
    }

    /// <summary>MemberPath se nastaví.</summary>
    [Fact]
    public void MemberAccess_Constructor_SetsMemberPath()
    {
        var expr = new MemberAccessExpression("Address.City", TypeModel.String);
        expr.MemberPath.Should().Be("Address.City");
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }

    /// <summary>Výchozí resultType je Object.</summary>
    [Fact]
    public void MemberAccess_DefaultResultType_IsObject()
    {
        var expr = new MemberAccessExpression("Name");
        expr.ResultType.BaseType.Should().Be(DataType.Object);
    }

    /// <summary>ResultType = whenTrue.ResultType.</summary>
    [Fact]
    public void Conditional_ResultType_EqualsWhenTrue()
    {
        var condition = new ConstantExpression(true);
        var whenTrue = new ConstantExpression("yes");
        var whenFalse = new ConstantExpression("no");
        var expr = new ConditionalExpression(condition, whenTrue, whenFalse);

        expr.ResultType.Should().Be(whenTrue.ResultType);
        expr.ResultType.BaseType.Should().Be(DataType.String);
    }

    /// <summary>BinaryOperator enum obsahuje všechny členy.</summary>
    [Fact]
    public void BinaryOperator_Enum_HasAllExpectedMembers()
    {
        var values = Enum.GetValues<BinaryOperator>();
        values.Should().Contain(BinaryOperator.Add);
        values.Should().Contain(BinaryOperator.Subtract);
        values.Should().Contain(BinaryOperator.Multiply);
        values.Should().Contain(BinaryOperator.Divide);
        values.Should().Contain(BinaryOperator.Modulo);
        values.Should().Contain(BinaryOperator.Equal);
        values.Should().Contain(BinaryOperator.NotEqual);
        values.Should().Contain(BinaryOperator.GreaterThan);
        values.Should().Contain(BinaryOperator.LessThan);
        values.Should().Contain(BinaryOperator.GreaterThanOrEqual);
        values.Should().Contain(BinaryOperator.LessThanOrEqual);
        values.Should().Contain(BinaryOperator.And);
        values.Should().Contain(BinaryOperator.Or);
        values.Should().Contain(BinaryOperator.Concat);
        values.Should().Contain(BinaryOperator.NullCoalesce);
    }

    /// <summary>UnaryOperator enum obsahuje všechny členy.</summary>
    [Fact]
    public void UnaryOperator_Enum_HasAllExpectedMembers()
    {
        var values = Enum.GetValues<UnaryOperator>();
        values.Should().Contain(UnaryOperator.Not);
        values.Should().Contain(UnaryOperator.Negate);
        values.Should().Contain(UnaryOperator.BitwiseNot);
        values.Should().Contain(UnaryOperator.Increment);
        values.Should().Contain(UnaryOperator.Decrement);
    }
}
