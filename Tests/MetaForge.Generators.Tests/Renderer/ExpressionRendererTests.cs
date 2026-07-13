using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Statements;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests.Renderer;

/// <summary>
/// Unit testy pro ExpressionRenderer — všechny typy expressionů.
/// Každá kategorie expressionů má vlastní testovací metodu.
/// PROP-048 — Generator Render Core Tests.
/// </summary>
public class ExpressionRendererTests
{
    private readonly ExpressionRenderer _renderer = new();

    private string Render(Expression expr)
    {
        var block = new BlockStatement(new ExpressionStatement { Expr = expr });
        return _renderer.RenderBodyOnly(block).TrimEnd(';').Trim();
    }

    // ========================================================================
    // CONSTANTS
    // ========================================================================

    [Fact]
    public void Constant_Null() =>
        Render(new ConstantExpression(null)).Should().Be("null");

    [Fact]
    public void Constant_String() =>
        Render(new ConstantExpression("hello")).Should().Be("\"hello\"");

    [Fact]
    public void Constant_Char() =>
        Render(new ConstantExpression('x')).Should().Be("'x'");

    [Fact]
    public void Constant_BoolTrue() =>
        Render(new ConstantExpression(true)).Should().Be("true");

    [Fact]
    public void Constant_Int() =>
        Render(new ConstantExpression(42)).Should().Be("42");

    [Fact]
    public void Constant_Decimal() =>
        Render(new ConstantExpression(3.14m)).Should().Be("3.14");

    // ========================================================================
    // BINARY
    // ========================================================================

    [Fact]
    public void Binary_Add() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Add,
            new MemberAccessExpression("b")
        )).Should().Be("(a + b)");

    [Fact]
    public void Binary_Subtract() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Subtract,
            new MemberAccessExpression("b")
        )).Should().Be("(a - b)");

    [Fact]
    public void Binary_Multiply() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Multiply,
            new MemberAccessExpression("b")
        )).Should().Be("(a * b)");

    [Fact]
    public void Binary_Divide() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Divide,
            new MemberAccessExpression("b")
        )).Should().Be("(a / b)");

    [Fact]
    public void Binary_Modulo() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Modulo,
            new MemberAccessExpression("b")
        )).Should().Be("(a % b)");

    [Fact]
    public void Binary_And() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.And,
            new MemberAccessExpression("b")
        )).Should().Be("(a && b)");

    [Fact]
    public void Binary_Or() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Or,
            new MemberAccessExpression("b")
        )).Should().Be("(a || b)");

    [Fact]
    public void Binary_Equal() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.Equal,
            new MemberAccessExpression("b")
        )).Should().Be("(a == b)");

    [Fact]
    public void Binary_NotEqual() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.NotEqual,
            new MemberAccessExpression("b")
        )).Should().Be("(a != b)");

    [Fact]
    public void Binary_LessThan() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.LessThan,
            new MemberAccessExpression("b")
        )).Should().Be("(a < b)");

    [Fact]
    public void Binary_GreaterThan() =>
        Render(new BinaryExpression(
            new MemberAccessExpression("a"),
            BinaryOperator.GreaterThan,
            new MemberAccessExpression("b")
        )).Should().Be("(a > b)");

    // ========================================================================
    // UNARY
    // ========================================================================

    [Fact]
    public void Unary_Not() =>
        Render(new UnaryExpression(UnaryOperator.Not, new MemberAccessExpression("flag")))
            .Should().Be("!flag");

    [Fact]
    public void Unary_Negate() =>
        Render(new UnaryExpression(UnaryOperator.Negate, new MemberAccessExpression("x")))
            .Should().Be("-x");

    [Fact]
    public void Unary_Increment() =>
        Render(new UnaryExpression(UnaryOperator.Increment, new MemberAccessExpression("x")))
            .Should().Be("++x");

    [Fact]
    public void Unary_Decrement() =>
        Render(new UnaryExpression(UnaryOperator.Decrement, new MemberAccessExpression("x")))
            .Should().Be("--x");

    // ========================================================================
    // METHOD CALL
    // ========================================================================

    [Fact]
    public void MethodCall_NoArgs() =>
        Render(new MethodCallExpression("Foo", Array.Empty<Expression>())).Should().Be("Foo()");

    [Fact]
    public void MethodCall_WithArgs() =>
        Render(new MethodCallExpression("Format", new Expression[]
        {
            new ConstantExpression("hello"),
            new ConstantExpression(42)
        })).Should().Be("Format(\"hello\", 42)");

    // ========================================================================
    // MEMBER ACCESS
    // ========================================================================

    [Fact]
    public void MemberAccess_Simple() =>
        Render(new MemberAccessExpression("obj.Property")).Should().Be("obj.Property");

    [Fact]
    public void MemberAccess_Nested() =>
        Render(new MemberAccessExpression("obj.Nested.Property")).Should().Be("obj.Nested.Property");

    // ========================================================================
    // CONDITIONAL
    // ========================================================================

    [Fact]
    public void Conditional_Simple() =>
        Render(new ConditionalExpression(
            new MemberAccessExpression("condition"),
            new ConstantExpression("yes"),
            new ConstantExpression("no")
        )).Should().Be("condition ? \"yes\" : \"no\"");

    // ========================================================================
    // NEW
    // ========================================================================

    [Fact]
    public void New_NoArgs() =>
        Render(new NewExpression("Foo")).Should().Be("new Foo()");

    [Fact]
    public void New_WithArgs() =>
        Render(new NewExpression("Point", new Expression[] { new ConstantExpression(3), new ConstantExpression(5) }))
            .Should().Be("new Point(3, 5)");

    [Fact]
    public void New_WithMemberBindings()
    {
        var expr = new NewExpression("Foo", new Expression[] { new ConstantExpression(42) })
        {
            MemberBindings = new[]
            {
                new MemberBinding { MemberName = "Name", Value = new ConstantExpression("test") }
            }
        };
        Render(expr).Should().Be("new Foo(42) { Name = \"test\" }");
    }

    // ========================================================================
    // AWAIT
    // ========================================================================

    [Fact]
    public void Await_Simple() =>
        Render(new AwaitExpression(new MemberAccessExpression("task")))
            .Should().Be("await task");

    // ========================================================================
    // CONVERSION
    // ========================================================================

    [Fact]
    public void Conversion_Explicit() =>
        Render(new ConversionExpression(TypeModel.Int32, new MemberAccessExpression("x")))
            .Should().Be("(int)x");

    // ========================================================================
    // DEFAULT
    // ========================================================================

    [Fact]
    public void Default_Simple() =>
        Render(new DefaultExpression(TypeModel.Int32)).Should().Be("default(int)");

    // ========================================================================
    // IS PATTERN
    // ========================================================================

    [Fact]
    public void IsPattern_Type() =>
        Render(new IsPatternExpression(
            new MemberAccessExpression("x"),
            PatternKind.Type,
            "string"
        )).Should().Be("x is string");

    [Fact]
    public void IsPattern_NotNull() =>
        Render(new IsPatternExpression(
            new MemberAccessExpression("x"),
            PatternKind.Null,
            null,
            true
        )).Should().Be("x is not null");

    // ========================================================================
    // LAMBDA
    // ========================================================================

    [Fact]
    public void Lambda_SingleParam() =>
        Render(new LambdaExpression(
            new[] { "x" },
            new BinaryExpression(
                new MemberAccessExpression("x"),
                BinaryOperator.Add,
                new ConstantExpression(1)
            )
        )).Should().Be("(x) => (x + 1)");

    [Fact]
    public void Lambda_MultiParam() =>
        Render(new LambdaExpression(
            new[] { "x", "y" },
            new BinaryExpression(
                new MemberAccessExpression("x"),
                BinaryOperator.Add,
                new MemberAccessExpression("y")
            )
        )).Should().Be("(x, y) => (x + y)");

    // ========================================================================
    // NULL COALESCING
    // ========================================================================

    [Fact]
    public void NullCoalescing_Simple() =>
        Render(new NullCoalescingExpression(
            new MemberAccessExpression("a"),
            new MemberAccessExpression("b")
        )).Should().Be("a ?? b");

    // ========================================================================
    // SWITCH EXPRESSION
    // ========================================================================

    [Fact]
    public void SwitchExpression_Simple() =>
        Render(new SwitchExpression(
            new MemberAccessExpression("x"),
            new SwitchArm[]
            {
                new() { Pattern = new ConstantExpression(1), Value = new ConstantExpression("one") },
                new() { Pattern = new MemberAccessExpression("_"), Value = new ConstantExpression("other") }
            }
        )).Should().Be("x switch { 1 => \"one\", _ => \"other\" }");

    // ========================================================================
    // EXTRA TYPES FOR MapType COVERAGE
    // ========================================================================

    [Fact]
    public void Default_Decimal() =>
        Render(new DefaultExpression(TypeModel.Decimal)).Should().Be("default(decimal)");

    [Fact]
    public void Default_DateTime() =>
        Render(new DefaultExpression(TypeModel.DateTime)).Should().Be("default(DateTime)");

    [Fact]
    public void Default_Object() =>
        Render(new DefaultExpression(TypeModel.Object)).Should().Be("default(object)");

    [Fact]
    public void Conversion_DecimalExplicit() =>
        Render(new ConversionExpression(TypeModel.Decimal, new MemberAccessExpression("price")))
            .Should().Be("(decimal)price");

    [Fact]
    public void Conversion_DoubleExplicit() =>
        Render(new ConversionExpression(
            new TypeModel { BaseType = DataType.Double },
            new MemberAccessExpression("x")
        )).Should().Be("(double)x");

    [Fact]
    public void Conversion_ImplicitIsNoop() =>
        Render(new ConversionExpression(TypeModel.Decimal, new MemberAccessExpression("price"), isExplicit: false))
            .Should().Be("price");

    [Fact]
    public void Default_String() =>
        Render(new DefaultExpression(TypeModel.String)).Should().Be("default(string)");

    // ========================================================================
    // EDGE CASES
    // ========================================================================

    [Fact]
    public void Binary_LongChain_IsCorrect() =>
        Render(new BinaryExpression(
            new BinaryExpression(
                new MemberAccessExpression("a"),
                BinaryOperator.Add,
                new MemberAccessExpression("b")
            ),
            BinaryOperator.Multiply,
            new MemberAccessExpression("c")
        )).Should().Be("((a + b) * c)");

    [Fact]
    public void Constant_NegativeInt() =>
        Render(new ConstantExpression(-5)).Should().Be("-5");

    [Fact]
    public void Unary_BitwiseNot() =>
        Render(new UnaryExpression(UnaryOperator.BitwiseNot, new MemberAccessExpression("flags")))
            .Should().Be("~flags");
}
