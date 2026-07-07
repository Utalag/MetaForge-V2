using FluentAssertions;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Statements;

namespace MetaForge.Core.Tests.Elements.Statements;

public class StatementTests
{
    /// <summary>BlockStatement — prázdný blok má StatementKind.Block.</summary>
    [Fact]
    public void BlockStatement_Empty_CorrectKind()
    {
        var block = new BlockStatement();
        block.StatementKind.Should().Be(StatementKind.Block);
        block.Statements.Should().BeEmpty();
    }

    /// <summary>BlockStatement — s statementy.</summary>
    [Fact]
    public void BlockStatement_WithStatements_ContainsAll()
    {
        var block = new BlockStatement(
            new ReturnStatement { Value = new ConstantExpression(42) },
            new ExpressionStatement { Expr = new MethodCallExpression("Log", [], null!) }
        );

        block.Statements.Should().HaveCount(2);
        block.Statements[0].Should().BeOfType<ReturnStatement>();
        block.Statements[1].Should().BeOfType<ExpressionStatement>();
    }

    /// <summary>ReturnStatement — s hodnotou.</summary>
    [Fact]
    public void ReturnStatement_WithValue_RendersReturnWithValue()
    {
        var ret = new ReturnStatement { Value = new ConstantExpression(42) };

        ret.StatementKind.Should().Be(StatementKind.Return);
        ret.Value.Should().NotBeNull();
    }

    /// <summary>ReturnStatement — bez hodnoty (void).</summary>
    [Fact]
    public void ReturnStatement_Void_ValueIsNull()
    {
        var ret = new ReturnStatement();

        ret.Value.Should().BeNull();
        ret.StatementKind.Should().Be(StatementKind.Return);
    }

    /// <summary>IfStatement — s true větví.</summary>
    [Fact]
    public void IfStatement_WithTrueBranch_HasBothConditionAndBranch()
    {
        var ifs = new IfStatement
        {
            Condition = new BinaryExpression(
                new ConstantExpression(1),
                BinaryOperator.GreaterThan,
                new ConstantExpression(0)),
            TrueBranch = new ReturnStatement { Value = new ConstantExpression("positive") }
        };

        ifs.StatementKind.Should().Be(StatementKind.If);
        ifs.Condition.Should().NotBeNull();
        ifs.TrueBranch.Should().NotBeNull();
        ifs.FalseBranch.Should().BeNull();
    }

    /// <summary>IfStatement — s else větví.</summary>
    [Fact]
    public void IfStatement_WithFalseBranch_HasElseBranch()
    {
        var ifs = new IfStatement
        {
            Condition = new ConstantExpression(true),
            TrueBranch = new ReturnStatement { Value = new ConstantExpression(1) },
            FalseBranch = new ReturnStatement { Value = new ConstantExpression(0) }
        };

        ifs.FalseBranch.Should().NotBeNull();
    }

    /// <summary>ForStatement — s tělem cyklu.</summary>
    [Fact]
    public void ForStatement_WithBody_HasAllProperties()
    {
        var forS = new ForStatement
        {
            Variable = "i",
            Start = new ConstantExpression(0),
            End = new ConstantExpression(10),
            Body = new ReturnStatement { Value = new ConstantExpression(42) }
        };

        forS.StatementKind.Should().Be(StatementKind.For);
        forS.Variable.Should().Be("i");
        forS.Start.Should().NotBeNull();
        forS.End.Should().NotBeNull();
        forS.Body.Should().NotBeNull();
    }

    /// <summary>WhileStatement — s podmínkou a tělem.</summary>
    [Fact]
    public void WhileStatement_WithBody_HasConditionAndBody()
    {
        var whileS = new WhileStatement
        {
            Condition = new BinaryExpression(
                new MemberAccessExpression("count"),
                BinaryOperator.GreaterThan,
                new ConstantExpression(0)),
            Body = new AssignmentStatement
            {
                Variable = "count",
                Value = new BinaryExpression(
                    new MemberAccessExpression("count"),
                    BinaryOperator.Subtract,
                    new ConstantExpression(1))
            }
        };

        whileS.StatementKind.Should().Be(StatementKind.While);
        whileS.Condition.Should().NotBeNull();
        whileS.Body.Should().NotBeNull();
    }

    /// <summary>AssignmentStatement — přiřazení.</summary>
    [Fact]
    public void AssignmentStatement_WithValue_SetsVariableAndValue()
    {
        var assign = new AssignmentStatement
        {
            Variable = "total",
            Value = new BinaryExpression(
                new MemberAccessExpression("price"),
                BinaryOperator.Multiply,
                new MemberAccessExpression("quantity"))
        };

        assign.StatementKind.Should().Be(StatementKind.Assignment);
        assign.Variable.Should().Be("total");
        assign.Value.Should().NotBeNull();
    }

    /// <summary>ExpressionStatement — výraz jako statement.</summary>
    [Fact]
    public void ExpressionStatement_WithMethodCall_HasExpression()
    {
        var stmt = new ExpressionStatement
        {
            Expr = new MethodCallExpression("list.Add", 
                [new ConstantExpression("item")], null!)
        };

        stmt.StatementKind.Should().Be(StatementKind.Expression);
        stmt.Expr.Should().NotBeNull();
        stmt.Expr.Should().BeOfType<MethodCallExpression>();
    }

    /// <summary>StatementKind enum — má všech 12 hodnot.</summary>
    [Fact]
    public void StatementKind_HasAllExpectedValues()
    {
        var values = Enum.GetValues<StatementKind>();
        values.Should().Contain(StatementKind.Block);
        values.Should().Contain(StatementKind.Return);
        values.Should().Contain(StatementKind.If);
        values.Should().Contain(StatementKind.For);
        values.Should().Contain(StatementKind.While);
        values.Should().Contain(StatementKind.Assignment);
        values.Should().Contain(StatementKind.Expression);
        values.Should().Contain(StatementKind.Switch);
        values.Should().Contain(StatementKind.ForEach);
        values.Should().Contain(StatementKind.TryCatch);
        values.Should().Contain(StatementKind.Using);
        values.Should().Contain(StatementKind.LocalFunction);
    }
}
