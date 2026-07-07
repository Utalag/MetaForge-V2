using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Generators.Tests;

/// <summary>
/// Testy renderování nově doplněných statementů a výrazů (switch, foreach, try/catch,
/// using, lokální funkce, lambda, new, default, conversion) přes <see cref="ExpressionRenderer"/>.
/// Validuje, že vyrenderovaný kód je syntakticky korektní C# (přes Roslyn).
/// </summary>
public class ExpressionRendererNewConstructsTests
{
    private readonly ExpressionRenderer _renderer = new();

    private static string Wrap(string body) => $$"""
        public class Wrapper
        {
            void Method()
            {{body}}
        }
        """;

    [Fact]
    public void Render_SwitchStatement_ProducesValidSyntax()
    {
        var switchS = new SwitchStatement
        {
            Selector = new MemberAccessExpression("status"),
            Cases =
            [
                new SwitchCase
                {
                    Pattern = PatternExpression.Constant(1),
                    Body = new ReturnStatement()
                },
                new SwitchCase
                {
                    Pattern = PatternExpression.Type("Customer", "c"),
                    Guard = new ConstantExpression(true),
                    Body = new ReturnStatement()
                },
                new SwitchCase
                {
                    Pattern = null,
                    Body = new ReturnStatement()
                }
            ]
        };

        var block = new BlockStatement(switchS);
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("switch (status)");
        rendered.Should().Contain("case 1:");
        rendered.Should().Contain("case Customer c when true:");
        rendered.Should().Contain("default:");
    }

    [Fact]
    public void Render_ForEachStatement_ProducesValidSyntax()
    {
        var forEach = new ForEachStatement
        {
            Variable = "item",
            ElementType = TypeModel.String,
            Collection = new MemberAccessExpression("items"),
            Body = new BlockStatement(new ExpressionStatement
            {
                Expr = new MethodCallExpression("Log", [new MemberAccessExpression("item")])
            })
        };

        var block = new BlockStatement(forEach);
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("foreach (String item in items)");
    }

    [Fact]
    public void Render_TryCatchStatement_ProducesValidSyntax()
    {
        var tryCatch = new TryCatchStatement
        {
            TryBlock = new BlockStatement(new ExpressionStatement
            {
                Expr = new MethodCallExpression("DoWork", [])
            }),
            CatchClauses =
            [
                new CatchClause
                {
                    ExceptionType = TypeModel.Object.WithCustomName("InvalidOperationException"),
                    VariableName = "ex",
                    Body = new BlockStatement(new ReturnStatement())
                }
            ],
            FinallyBlock = new BlockStatement(new ExpressionStatement
            {
                Expr = new MethodCallExpression("Cleanup", [])
            })
        };

        var block = new BlockStatement(tryCatch);
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("try");
        rendered.Should().Contain("catch (InvalidOperationException ex)");
        rendered.Should().Contain("finally");
    }

    [Fact]
    public void Render_UsingStatement_BlockForm_ProducesValidSyntax()
    {
        var usingS = new UsingStatement
        {
            Variable = "stream",
            VariableType = TypeModel.Object.WithCustomName("FileStream"),
            Resource = new NewExpression("FileStream"),
            Body = new BlockStatement()
        };

        var block = new BlockStatement(usingS);
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("using (FileStream stream = new FileStream())");
    }

    [Fact]
    public void Render_UsingStatement_DeclarationForm_ProducesValidSyntax()
    {
        var usingS = new UsingStatement
        {
            Variable = "stream",
            VariableType = TypeModel.Object.WithCustomName("FileStream"),
            Resource = new NewExpression("FileStream"),
        };

        var block = new BlockStatement(usingS);
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("using FileStream stream = new FileStream();");
    }

    [Fact]
    public void Render_LocalFunctionStatement_ProducesValidSyntax()
    {
        var localFunc = new LocalFunctionStatement
        {
            Name = "Square",
            ReturnType = TypeModel.Int32,
            Parameters = [new ParameterElement { Name = "x", Type = TypeModel.Int32 }],
            Body = new BlockStatement(
                new ReturnStatement
                {
                    Value = new BinaryExpression(
                        new MemberAccessExpression("x"),
                        BinaryOperator.Multiply,
                        new MemberAccessExpression("x"))
                })
        };

        var block = new BlockStatement(localFunc, new ReturnStatement { Value = new ConstantExpression(0) });
        var rendered = _renderer.Render(block, 2);
        var source = Wrap(rendered);

        SyntaxValidator.IsValid(source, out var diagnostics).Should().BeTrue(diagnostics);
        rendered.Should().Contain("Int32 Square(Int32 x)");
    }

    [Fact]
    public void Render_LambdaExpression_ProducesValidSyntax()
    {
        var assign = new AssignmentStatement
        {
            Variable = "getName",
            Value = new LambdaExpression(["x"], new MemberAccessExpression("x.FirstName"))
        };

        var expr = _renderer.RenderExpression(assign.Value);
        expr.Should().Be("x => x.FirstName");
    }

    [Fact]
    public void Render_NewExpression_WithInitializers_ProducesValidSyntax()
    {
        var newExpr = new NewExpression(
            "Customer",
            arguments: [],
            initializers: [new NamedArgument("Name", new ConstantExpression("John"))]);

        var rendered = _renderer.RenderExpression(newExpr);
        rendered.Should().Be("new Customer() { Name = \"John\" }");
    }

    [Fact]
    public void Render_DefaultExpression_ProducesValidSyntax()
    {
        var defaultExpr = new DefaultExpression(TypeModel.Int32);
        var rendered = _renderer.RenderExpression(defaultExpr);
        rendered.Should().Be("default(Int32)");
    }

    [Fact]
    public void Render_ConversionExpression_Cast_ProducesValidSyntax()
    {
        var conversion = new ConversionExpression(new MemberAccessExpression("price"), TypeModel.Decimal);
        var rendered = _renderer.RenderExpression(conversion);
        rendered.Should().Be("((Decimal)price)");
    }

    [Fact]
    public void Render_MethodCall_WithNamedArguments_ProducesValidSyntax()
    {
        var call = MethodCallExpression.WithNamedArguments(
            "Configure",
            [new NamedArgument("timeout", new ConstantExpression(30))]);

        var rendered = _renderer.RenderExpression(call);
        rendered.Should().Be("Configure(timeout: 30)");
    }
}
