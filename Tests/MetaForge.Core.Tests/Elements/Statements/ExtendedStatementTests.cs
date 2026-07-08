using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Tests.Elements.Statements;

public class ExtendedStatementTests
{
    [Fact]
    public void SwitchStatement_CreatesWithSelectorAndCases()
    {
        var sw = new SwitchStatement(new MemberAccessExpression("status"))
        {
            Cases =
            {
                new SwitchCase(new ConstantExpression(1),
                    new BlockStatement { Statements = { new ReturnStatement { Value = new ConstantExpression("one") } } }),
                new SwitchCase(new ConstantExpression(2),
                    new BlockStatement { Statements = { new ReturnStatement { Value = new ConstantExpression("two") } } }),
            }
        };

        sw.StatementKind.Should().Be(StatementKind.Switch);
        sw.Cases.Should().HaveCount(2);
        sw.Cases[0].Pattern.Should().BeOfType<ConstantExpression>();
    }

    [Fact]
    public void ForEachStatement_CreatesCorrectly()
    {
        var fe = new ForEachStatement("item", new MemberAccessExpression("collection"),
            new BlockStatement
            {
                Statements =
                {
                    new ExpressionStatement { Expr = new MethodCallExpression("Process", [], null) }
                }
            });

        fe.StatementKind.Should().Be(StatementKind.ForEach);
        fe.VariableName.Should().Be("item");
        fe.Collection.Should().BeOfType<MemberAccessExpression>();
    }

    [Fact]
    public void TryCatchStatement_CreatesWithCatchAndFinally()
    {
        var tc = new TryCatchStatement(
            new BlockStatement(),
            new CatchClause("Exception", "ex", new BlockStatement
            {
                Statements =
                {
                    new ReturnStatement { Value = new ConstantExpression(null) }
                }
            }));

        tc.StatementKind.Should().Be(StatementKind.TryCatch);
        tc.Catches.Should().HaveCount(1);
    }

    [Fact]
    public void UsingStatement_CreatesCorrectly()
    {
        var us = new UsingStatement(
            new AssignmentStatement { Variable = "reader", Value = new MemberAccessExpression("CreateReader") },
            new BlockStatement());

        us.StatementKind.Should().Be(StatementKind.Using);
    }

    [Fact]
    public void LocalFunctionStatement_CreatesWithMethod()
    {
        var lf = new LocalFunctionStatement(
            new MethodElement { Name = "Helper", ReturnType = TypeModel.Int32 });

        lf.StatementKind.Should().Be(StatementKind.LocalFunction);
        lf.Function.Name.Should().Be("Helper");
    }

    [Fact]
    public void AllNewKinds_AreInEnum()
    {
        var expected = new[] {
            StatementKind.Switch, StatementKind.ForEach, StatementKind.TryCatch,
            StatementKind.Using, StatementKind.UsingDeclaration, StatementKind.LocalFunction
        };
        expected.Should().AllSatisfy(k => k.ToString().Should().NotBeNullOrEmpty());
    }
}
