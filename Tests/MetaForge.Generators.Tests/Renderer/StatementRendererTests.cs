using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests.Renderer;

/// <summary>
/// Unit testy pro ExpressionRenderer — všechny typy statementů.
/// PROP-048 — Generator Render Core Tests.
/// </summary>
public class StatementRendererTests
{
    private readonly ExpressionRenderer _renderer = new();

    // ========================================================================
    // RETURN
    // ========================================================================

    [Fact]
    public void Return_WithValue()
    {
        var block = new BlockStatement(new ReturnStatement { Value = new ConstantExpression(42) });
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Be("return 42;");
    }

    [Fact]
    public void Return_WithoutValue()
    {
        var block = new BlockStatement(new ReturnStatement());
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Be("return;");
    }

    // ========================================================================
    // IF/ELSE
    // ========================================================================

    [Fact]
    public void If_Simple()
    {
        var block = new BlockStatement(
            new IfStatement
            {
                Condition = new MemberAccessExpression("flag"),
                TrueBranch = new BlockStatement(
                    new ReturnStatement { Value = new ConstantExpression("yes") }
                )
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("if (flag)");
        result.Should().Contain("return \"yes\";");
    }

    [Fact]
    public void If_WithElse()
    {
        var block = new BlockStatement(
            new IfStatement
            {
                Condition = new MemberAccessExpression("flag"),
                TrueBranch = new BlockStatement(
                    new ReturnStatement { Value = new ConstantExpression("yes") }
                ),
                FalseBranch = new BlockStatement(
                    new ReturnStatement { Value = new ConstantExpression("no") }
                )
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("if (flag)");
        result.Should().Contain("else");
        result.Should().Contain("return \"yes\";");
        result.Should().Contain("return \"no\";");
    }

    // ========================================================================
    // FOR
    // ========================================================================

    [Fact]
    public void For_Loop()
    {
        var block = new BlockStatement(
            new ForStatement
            {
                Variable = "i",
                Start = new ConstantExpression(0),
                End = new ConstantExpression(10),
                Body = new BlockStatement(
                    new ExpressionStatement { Expr = new MethodCallExpression("Process", Array.Empty<Expression>()) }
                )
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("for (int i = 0; i < 10; i++)");
        result.Should().Contain("Process();");
    }

    // ========================================================================
    // WHILE
    // ========================================================================

    [Fact]
    public void While_Loop()
    {
        var block = new BlockStatement(
            new WhileStatement
            {
                Condition = new MemberAccessExpression("running"),
                Body = new BlockStatement(
                    new ExpressionStatement { Expr = new MethodCallExpression("Tick", Array.Empty<Expression>()) }
                )
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("while (running)");
        result.Should().Contain("Tick();");
    }

    // ========================================================================
    // ASSIGNMENT
    // ========================================================================

    [Fact]
    public void Assignment_Simple()
    {
        var block = new BlockStatement(
            new AssignmentStatement
            {
                Variable = "count",
                Value = new ConstantExpression(42)
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Be("count = 42;");
    }

    // ========================================================================
    // EXPRESSION STATEMENT
    // ========================================================================

    [Fact]
    public void ExpressionStatement_MethodCall()
    {
        var block = new BlockStatement(
            new ExpressionStatement
            {
                Expr = new MethodCallExpression("Execute", new Expression[]
                {
                    new MemberAccessExpression("ctx")
                })
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Be("Execute(ctx);");
    }

    // ========================================================================
    // FOREACH
    // ========================================================================

    [Fact]
    public void ForEach_Loop()
    {
        var block = new BlockStatement(
            new ForEachStatement("item",
                new MemberAccessExpression("items"),
                new BlockStatement(
                    new ExpressionStatement { Expr = new MethodCallExpression("Process", Array.Empty<Expression>()) }
                )
            )
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("foreach (var item in items)");
        result.Should().Contain("Process();");
    }

    // ========================================================================
    // SWITCH
    // ========================================================================

    [Fact]
    public void Switch_WithCases()
    {
        var block = new BlockStatement(
            new SwitchStatement(
                new MemberAccessExpression("status"),
                new SwitchCase
                {
                    Pattern = new ConstantExpression(1),
                    Body = new BlockStatement(
                        new ReturnStatement { Value = new ConstantExpression("active") }
                    )
                }
            )
            {
                DefaultCase = new BlockStatement(
                    new ReturnStatement { Value = new ConstantExpression("unknown") }
                )
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("switch (status)");
        result.Should().Contain("case 1:");
        result.Should().Contain("default:");
    }

    // ========================================================================
    // TRY/CATCH
    // ========================================================================

    [Fact]
    public void TryCatch_Simple()
    {
        var block = new BlockStatement(
            new TryCatchStatement
            {
                TryBody = new BlockStatement(
                    new ExpressionStatement { Expr = new MethodCallExpression("DoWork", Array.Empty<Expression>()) }
                ),
                Catches =
                {
                    new CatchClause
                    {
                        ExceptionType = "Exception",
                        VariableName = "ex",
                        Body = new BlockStatement(
                            new ExpressionStatement { Expr = new MethodCallExpression("HandleError", Array.Empty<Expression>()) }
                        )
                    }
                }
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("try");
        result.Should().Contain("catch (Exception ex)");
        result.Should().Contain("DoWork();");
        result.Should().Contain("HandleError();");
    }

    // ========================================================================
    // USING
    // ========================================================================

    [Fact]
    public void Using_Block()
    {
        var block = new BlockStatement(
            new UsingStatement(
                new AssignmentStatement { Variable = "conn", Value = new NewExpression("SqlConnection") },
                new BlockStatement(
                    new ExpressionStatement { Expr = new MethodCallExpression("Execute", Array.Empty<Expression>()) }
                )
            )
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("using (conn = new SqlConnection())");
        result.Should().Contain("Execute();");
    }

    [Fact]
    public void UsingDeclaration()
    {
        var block = new BlockStatement(
            new UsingDeclarationStatement
            {
                VariableName = "conn",
                Initializer = new MethodCallExpression("GetConnection", Array.Empty<Expression>())
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Be("using var conn = GetConnection();");
    }

    // ========================================================================
    // LOCAL FUNCTION
    // ========================================================================

    [Fact]
    public void LocalFunction_Simple()
    {
        var localFunc = new LocalFunctionStatement
        {
            Function = new MethodElement
            {
                Name = "Add",
                ReturnType = TypeModel.Int32,
                Parameters =
                {
                    new ParameterElement { Name = "a", Type = TypeModel.Int32 },
                    new ParameterElement { Name = "b", Type = TypeModel.Int32 }
                },
                Body = new BlockStatement(
                    new ReturnStatement
                    {
                        Value = new BinaryExpression(
                            new MemberAccessExpression("a"),
                            BinaryOperator.Add,
                            new MemberAccessExpression("b")
                        )
                    }
                )
            }
        };
        var block = new BlockStatement(localFunc,
            new ExpressionStatement
            {
                Expr = new MethodCallExpression("Add", new Expression[] { new ConstantExpression(1), new ConstantExpression(2) })
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("int Add(int a, int b)");
        result.Should().Contain("return (a + b);");
    }

    // ========================================================================
    // EDGE CASES
    // ========================================================================

    [Fact]
    public void RenderBodyOnly_EmptyBlock_ReturnsEmpty()
    {
        var block = new BlockStatement();
        var result = _renderer.RenderBodyOnly(block);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ForStatement_NullBody_DoesNotCrash()
    {
        var block = new BlockStatement(
            new ForStatement
            {
                Variable = "i",
                Start = new ConstantExpression(0),
                End = new ConstantExpression(5),
                Body = null
            }
        );
        var result = _renderer.RenderBodyOnly(block);
        result.Should().Contain("for (int i = 0; i < 5; i++)");
    }
}
