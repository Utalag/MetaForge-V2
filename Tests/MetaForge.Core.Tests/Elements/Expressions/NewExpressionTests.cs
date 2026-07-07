using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;

namespace MetaForge.Core.Tests.Elements.Expressions;

public class NewExpressionTests
{
    /// <summary>LambdaExpression — expression-bodied lambda s jedním parametrem.</summary>
    [Fact]
    public void LambdaExpression_SingleParameter_HasBodyAndParameter()
    {
        var lambda = new LambdaExpression(["x"], new MemberAccessExpression("x.FirstName"));

        lambda.ExpressionKind.Should().Be(ExpressionKind.Lambda);
        lambda.Parameters.Should().ContainSingle().Which.Should().Be("x");
        lambda.Body.Should().NotBeNull();
    }

    /// <summary>LambdaExpression — s více parametry.</summary>
    [Fact]
    public void LambdaExpression_MultipleParameters_HasAllParameters()
    {
        var lambda = new LambdaExpression(
            ["a", "b"],
            new BinaryExpression(new MemberAccessExpression("a"), BinaryOperator.Add, new MemberAccessExpression("b")));

        lambda.Parameters.Should().HaveCount(2);
    }

    /// <summary>NewExpression — konstruktor bez argumentů, bez initializeru.</summary>
    [Fact]
    public void NewExpression_NoArguments_EmptyArgumentsAndInitializers()
    {
        var expr = new NewExpression("Customer");

        expr.ExpressionKind.Should().Be(ExpressionKind.New);
        expr.TypeName.Should().Be("Customer");
        expr.Arguments.Should().BeEmpty();
        expr.Initializers.Should().BeEmpty();
    }

    /// <summary>NewExpression — s konstruktorovými argumenty a object initializerem.</summary>
    [Fact]
    public void NewExpression_WithArgumentsAndInitializers_HasBoth()
    {
        var expr = new NewExpression(
            "Customer",
            arguments: [new NamedArgument(new ConstantExpression("John"))],
            initializers: [new NamedArgument("Age", new ConstantExpression(30))]);

        expr.Arguments.Should().HaveCount(1);
        expr.Initializers.Should().HaveCount(1);
        expr.Initializers[0].Name.Should().Be("Age");
    }

    /// <summary>DefaultExpression — default(T) nese cílový typ jako ResultType.</summary>
    [Fact]
    public void DefaultExpression_SetsResultTypeFromType()
    {
        var expr = new DefaultExpression(TypeModel.Int32);

        expr.ExpressionKind.Should().Be(ExpressionKind.Default);
        expr.ResultType.Should().Be(TypeModel.Int32);
    }

    /// <summary>ConversionExpression — explicitní cast.</summary>
    [Fact]
    public void ConversionExpression_Cast_DefaultKind()
    {
        var expr = new ConversionExpression(new MemberAccessExpression("price"), TypeModel.Decimal);

        expr.ExpressionKind.Should().Be(ExpressionKind.Conversion);
        expr.ConversionKind.Should().Be(ConversionKind.Cast);
        expr.TargetType.Should().Be(TypeModel.Decimal);
    }

    /// <summary>ConversionExpression — `as` konverze.</summary>
    [Fact]
    public void ConversionExpression_As_SetsConversionKind()
    {
        var expr = new ConversionExpression(
            new MemberAccessExpression("obj"), TypeModel.Object.WithCustomName("Customer"), ConversionKind.As);

        expr.ConversionKind.Should().Be(ConversionKind.As);
    }

    /// <summary>NamedArgument — poziční argument nemá jméno.</summary>
    [Fact]
    public void NamedArgument_Positional_NameIsNull()
    {
        NamedArgument arg = new ConstantExpression(42);

        arg.Name.Should().BeNull();
        arg.Value.Should().BeOfType<ConstantExpression>();
    }

    /// <summary>NamedArgument — pojmenovaný argument.</summary>
    [Fact]
    public void NamedArgument_Named_HasName()
    {
        var arg = new NamedArgument("count", new ConstantExpression(5));

        arg.Name.Should().Be("count");
    }

    /// <summary>MethodCallExpression — s pojmenovanými argumenty přes WithNamedArguments.</summary>
    [Fact]
    public void MethodCall_WithNamedArguments_PreservesNames()
    {
        var expr = MethodCallExpression.WithNamedArguments(
            "Configure",
            [new NamedArgument("timeout", new ConstantExpression(30)), new NamedArgument(new ConstantExpression(true))]);

        expr.Arguments.Should().HaveCount(2);
        expr.Arguments[0].Name.Should().Be("timeout");
        expr.Arguments[1].Name.Should().BeNull();
    }

    /// <summary>MethodCallExpression — poziční konstruktor stále funguje a obaluje argumenty jako NamedArgument.</summary>
    [Fact]
    public void MethodCall_PositionalConstructor_WrapsArgumentsWithoutNames()
    {
        var expr = new MethodCallExpression("string.IsNullOrEmpty", [new ConstantExpression("x")]);

        expr.Arguments.Should().ContainSingle();
        expr.Arguments[0].Name.Should().BeNull();
    }

    /// <summary>PatternExpression — Discard vzor.</summary>
    [Fact]
    public void PatternExpression_Discard_HasDiscardKind()
    {
        var pattern = PatternExpression.Discard();

        pattern.ExpressionKind.Should().Be(ExpressionKind.Pattern);
        pattern.PatternKind.Should().Be(PatternKind.Discard);
    }

    /// <summary>PatternExpression — Constant vzor nese hodnotu.</summary>
    [Fact]
    public void PatternExpression_Constant_HasValue()
    {
        var pattern = PatternExpression.Constant(42);

        pattern.PatternKind.Should().Be(PatternKind.Constant);
        pattern.ConstantValue.Should().Be(42);
    }

    /// <summary>PatternExpression — Type vzor s bindingem.</summary>
    [Fact]
    public void PatternExpression_Type_HasTypeNameAndBinding()
    {
        var pattern = PatternExpression.Type("Customer", "c");

        pattern.PatternKind.Should().Be(PatternKind.Type);
        pattern.TypeName.Should().Be("Customer");
        pattern.BindingName.Should().Be("c");
    }

    /// <summary>ExpressionKind enum obsahuje nově doplněné hodnoty.</summary>
    [Fact]
    public void ExpressionKind_ContainsNewlyImplementedMembers()
    {
        var values = Enum.GetValues<ExpressionKind>();
        values.Should().Contain(ExpressionKind.Lambda);
        values.Should().Contain(ExpressionKind.New);
        values.Should().Contain(ExpressionKind.Default);
        values.Should().Contain(ExpressionKind.Conversion);
        values.Should().Contain(ExpressionKind.Pattern);
    }
}
