using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Tests.Elements.Statements;

public class NewStatementTests
{
    /// <summary>SwitchStatement — case větev s konstantním vzorem.</summary>
    [Fact]
    public void SwitchStatement_WithConstantCase_HasSelectorAndCases()
    {
        var switchS = new SwitchStatement
        {
            Selector = new MemberAccessExpression("status"),
            Cases =
            [
                new SwitchCase
                {
                    Pattern = PatternExpression.Constant(1),
                    Body = new ReturnStatement { Value = new ConstantExpression("active") }
                },
                new SwitchCase
                {
                    Pattern = null, // default
                    Body = new ReturnStatement { Value = new ConstantExpression("unknown") }
                }
            ]
        };

        switchS.StatementKind.Should().Be(StatementKind.Switch);
        switchS.Selector.Should().NotBeNull();
        switchS.Cases.Should().HaveCount(2);
        switchS.Cases[0].Pattern!.PatternKind.Should().Be(PatternKind.Constant);
        switchS.Cases[1].Pattern.Should().BeNull();
    }

    /// <summary>SwitchStatement — case s type pattern a guard clause.</summary>
    [Fact]
    public void SwitchStatement_WithTypePatternAndGuard_HasBindingAndGuard()
    {
        var switchCase = new SwitchCase
        {
            Pattern = PatternExpression.Type("Customer", "c"),
            Guard = new BinaryExpression(
                new MemberAccessExpression("c.Age"),
                BinaryOperator.GreaterThan,
                new ConstantExpression(18)),
            Body = new ReturnStatement { Value = new ConstantExpression(true) }
        };

        switchCase.Pattern!.PatternKind.Should().Be(PatternKind.Type);
        switchCase.Pattern.TypeName.Should().Be("Customer");
        switchCase.Pattern.BindingName.Should().Be("c");
        switchCase.Guard.Should().NotBeNull();
    }

    /// <summary>ForEachStatement — iterace přes kolekci.</summary>
    [Fact]
    public void ForEachStatement_WithCollection_HasAllProperties()
    {
        var forEach = new ForEachStatement
        {
            Variable = "item",
            ElementType = TypeModel.String,
            Collection = new MemberAccessExpression("items"),
            Body = new ExpressionStatement
            {
                Expr = new MethodCallExpression("Log", [new MemberAccessExpression("item")])
            }
        };

        forEach.StatementKind.Should().Be(StatementKind.ForEach);
        forEach.Variable.Should().Be("item");
        forEach.ElementType.Should().Be(TypeModel.String);
        forEach.Collection.Should().NotBeNull();
        forEach.Body.Should().NotBeNull();
    }

    /// <summary>TryCatchStatement — s catch klauzulí a finally blokem.</summary>
    [Fact]
    public void TryCatchStatement_WithCatchAndFinally_HasAllParts()
    {
        var tryCatch = new TryCatchStatement
        {
            TryBlock = new BlockStatement(
                new ExpressionStatement { Expr = new MethodCallExpression("DoWork", []) }),
            CatchClauses =
            [
                new CatchClause
                {
                    ExceptionType = TypeModel.Object.WithCustomName("InvalidOperationException"),
                    VariableName = "ex",
                    Body = new BlockStatement(new ReturnStatement { Value = new ConstantExpression(false) })
                }
            ],
            FinallyBlock = new BlockStatement(
                new ExpressionStatement { Expr = new MethodCallExpression("Cleanup", []) })
        };

        tryCatch.StatementKind.Should().Be(StatementKind.TryCatch);
        tryCatch.TryBlock.Should().NotBeNull();
        tryCatch.CatchClauses.Should().HaveCount(1);
        tryCatch.CatchClauses[0].VariableName.Should().Be("ex");
        tryCatch.FinallyBlock.Should().NotBeNull();
    }

    /// <summary>TryCatchStatement — catch bez typu a bez finally.</summary>
    [Fact]
    public void TryCatchStatement_BareCatch_NoTypeNoFinally()
    {
        var tryCatch = new TryCatchStatement
        {
            TryBlock = new BlockStatement(),
            CatchClauses = [new CatchClause { Body = new BlockStatement() }]
        };

        tryCatch.CatchClauses[0].ExceptionType.Should().BeNull();
        tryCatch.FinallyBlock.Should().BeNull();
    }

    /// <summary>UsingStatement — bloková forma s tělem.</summary>
    [Fact]
    public void UsingStatement_BlockForm_HasBody()
    {
        var usingS = new UsingStatement
        {
            Variable = "stream",
            Resource = new NewExpression("FileStream"),
            Body = new BlockStatement()
        };

        usingS.StatementKind.Should().Be(StatementKind.Using);
        usingS.Body.Should().NotBeNull();
    }

    /// <summary>UsingStatement — deklarační forma bez těla.</summary>
    [Fact]
    public void UsingStatement_DeclarationForm_BodyIsNull()
    {
        var usingS = new UsingStatement
        {
            Variable = "stream",
            Resource = new NewExpression("FileStream"),
        };

        usingS.Body.Should().BeNull();
    }

    /// <summary>LocalFunctionStatement — s parametry a návratovým typem.</summary>
    [Fact]
    public void LocalFunctionStatement_WithParameters_HasAllProperties()
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

        localFunc.StatementKind.Should().Be(StatementKind.LocalFunction);
        localFunc.Name.Should().Be("Square");
        localFunc.Parameters.Should().HaveCount(1);
        localFunc.IsStatic.Should().BeFalse();
        localFunc.IsAsync.Should().BeFalse();
    }
}
