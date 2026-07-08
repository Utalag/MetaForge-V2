// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — InvariantExpressionTests
// Unit tests for the boolean AST in InvariantExpression.
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using MetaForge.Core.Specifications;
using static MetaForge.Core.Specifications.InvariantExpressionBuilder;

namespace MetaForge.Core.Tests.Specifications;

public class InvariantExpressionTests
{
    [Fact]
    public void PropertyRef_StoresPath()
    {
        var prop = new PropertyRef("$.IsAbstract");
        Assert.Equal("$.IsAbstract", prop.Path);
    }

    [Fact]
    public void PropertyRef_Eq_CreatesEqExpression()
    {
        var eq = Prop("$.IsAbstract").Eq(true);

        Assert.IsType<EqExpression>(eq);
        var eqExpr = (EqExpression)eq;
        Assert.IsType<PropertyRef>(eqExpr.Left);
        Assert.IsType<ConstantExpression>(eqExpr.Right);
        Assert.Equal("$.IsAbstract", ((PropertyRef)eqExpr.Left).Path);
        Assert.Equal(true, ((ConstantExpression)eqExpr.Right).Value);
    }

    [Fact]
    public void ConstantExpression_StoresValue()
    {
        var c = Const(42);
        Assert.Equal(42, c.Value);
    }

    [Fact]
    public void NotExpression_WrapsInner()
    {
        var not = new NotExpression(Prop("$.IsAbstract"));
        Assert.IsType<PropertyRef>(not.Inner);
    }

    [Fact]
    public void AndExpression_CombinesItems()
    {
        var and = new AndExpression(Prop("$.A"), Prop("$.B"));
        Assert.Equal(2, and.Items.Count);
    }

    [Fact]
    public void OrExpression_CombinesItems()
    {
        var or = new OrExpression(Prop("$.A"), Prop("$.B"));
        Assert.Equal(2, or.Items.Count);
    }

    [Fact]
    public void ExistsExpression_StoresPath()
    {
        var exists = new ExistsExpression("$.Body");
        Assert.Equal("$.Body", exists.Path);
    }

    [Fact]
    public void ExistsExpression_Not_ReturnsNotExpression()
    {
        var not = new ExistsExpression("$.Body").Not();
        Assert.IsType<NotExpression>(not);
        Assert.IsType<ExistsExpression>(not.Inner);
    }

    [Fact]
    public void Implies_NormalisesToOrNot()
    {
        var a = Prop("$.IsAbstract").Eq(true);
        var b = new NotExpression(new ExistsExpression("$.Body"));

        var implies = InvariantExpressionBuilder.Implies(a, b);

        Assert.IsType<OrExpression>(implies);
        var or = (OrExpression)implies;
        Assert.Equal(2, or.Items.Count);
        Assert.IsType<NotExpression>(or.Items[0]);
        // The first item should be Not(a)
        Assert.IsType<NotExpression>(or.Items[0]);
    }

    [Fact]
    public void AndExpression_Empty_HandledGracefully()
    {
        var and = new AndExpression();
        Assert.Empty(and.Items);
    }

    [Fact]
    public void OrExpression_Empty_HandledGracefully()
    {
        var or = new OrExpression();
        Assert.Empty(or.Items);
    }
}
