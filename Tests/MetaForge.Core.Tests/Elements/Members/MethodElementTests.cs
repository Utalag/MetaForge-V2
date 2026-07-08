using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Core.Tests.Elements.Members;

public class MethodElementTests
{
    /// <summary>Výchozí ReturnType je TypeModel.Void.</summary>
    [Fact]
    public void ReturnType_Default_IsVoid()
    {
        var method = new MethodElement();
        method.ReturnType.BaseType.Should().Be(DataType.Void);
    }

    /// <summary>TotalCoin = Coin při prázdných Parameters.</summary>
    [Fact]
    public void TotalCoin_NoParameters_ReturnsCoin()
    {
        var method = new MethodElement { Coin = 10 };
        method.TotalCoin.Should().Be(10);
    }

    /// <summary>TotalCoin zahrnuje Coin všech Parameters.</summary>
    [Fact]
    public void TotalCoin_WithParameters_IncludesSum()
    {
        var method = new MethodElement { Coin = 5 };
        method.Parameters.Add(new ParameterElement { Coin = 2 });
        method.Parameters.Add(new ParameterElement { Coin = 3 });

        method.TotalCoin.Should().Be(10);
    }

    /// <summary>IsStatic je false.</summary>
    [Fact]
    public void IsStatic_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsStatic.Should().BeFalse();
    }

    /// <summary>IsAsync je false.</summary>
    [Fact]
    public void IsAsync_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsAsync.Should().BeFalse();
    }

    /// <summary>IsAbstract je false.</summary>
    [Fact]
    public void IsAbstract_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsAbstract.Should().BeFalse();
    }

    /// <summary>IsVirtual je false.</summary>
    [Fact]
    public void IsVirtual_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsVirtual.Should().BeFalse();
    }

    /// <summary>IsOverride je false.</summary>
    [Fact]
    public void IsOverride_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsOverride.Should().BeFalse();
    }

    /// <summary>Body je null.</summary>
    [Fact]
    public void Body_Default_IsNull()
    {
        var method = new MethodElement();
        method.Body.Should().BeNull();
    }

    /// <summary>Výchozí Coin je 5.</summary>
    [Fact]
    public void Coin_Default_IsFive()
    {
        var method = new MethodElement();
        method.Coin.Should().Be(5);
    }

    /// <summary>Parameters je prázdný seznam.</summary>
    [Fact]
    public void Parameters_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.Parameters.Should().BeEmpty();
    }

    /// <summary>Attributes je prázdný seznam.</summary>
    [Fact]
    public void Attributes_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.Attributes.Should().BeEmpty();
    }

    // === PROP-035: Nové vlastnosti ===

    /// <summary>IsExtension je ve výchozím stavu false.</summary>
    [Fact]
    public void IsExtension_Default_IsFalse()
    {
        var method = new MethodElement();
        method.IsExtension.Should().BeFalse();
    }

    /// <summary>ExpressionBody je ve výchozím stavu null.</summary>
    [Fact]
    public void ExpressionBody_Default_IsNull()
    {
        var method = new MethodElement();
        method.ExpressionBody.Should().BeNull();
    }

    /// <summary>TypeParameters je ve výchozím stavu prázdný seznam.</summary>
    [Fact]
    public void TypeParameters_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.TypeParameters.Should().BeEmpty();
    }

    /// <summary>TypeConstraints je ve výchozím stavu prázdný seznam.</summary>
    [Fact]
    public void TypeConstraints_Default_IsEmpty()
    {
        var method = new MethodElement();
        method.TypeConstraints.Should().BeEmpty();
    }

    /// <summary>Generic factory vytvoří generickou metodu s TypeParameters a TypeConstraints.</summary>
    [Fact]
    public void GenericFactory_CreatesGenericMethod()
    {
        var constraint = GenericConstraint.Class("T");
        var method = MethodElement.Generic(
            "Swap", TypeModel.Void,
            new[] { "T" },
            new[] { constraint });

        method.TypeParameters.Should().BeEquivalentTo(["T"]);
        method.TypeConstraints.Should().HaveCount(1);
        method.TypeConstraints[0].TypeParameterName.Should().Be("T");
        method.TypeConstraints[0].Constraints.Should().Contain(ConstraintKind.Class);
    }

    /// <summary>Invariant: IsAsync a IsAbstract současně — sémanticky nesmyslné (nelze await abstraktní metodu).</summary>
    [Fact]
    public void Invariant_AsyncAndAbstract_BothTrue()
    {
        var method = MethodElement.Abstract("Get", TypeModel.String);
        method.IsAsync = true;

        // Sémanticky nevalidní, ale model to dovoluje — validace je na vyšší vrstvě.
        method.IsAsync.Should().BeTrue();
        method.IsAbstract.Should().BeTrue();
    }

    /// <summary>Invariant: Body a ExpressionBody současně — ambiguous.</summary>
    [Fact]
    public void Invariant_BodyAndExpressionBody_BothSet()
    {
        var method = MethodElement.Basic("Double");
        method.Body = new BlockStatement();
        method.ExpressionBody = new BinaryExpression(
            new MemberAccessExpression("x"),
            BinaryOperator.Multiply,
            new ConstantExpression(2));

        method.Body.Should().NotBeNull();
        method.ExpressionBody.Should().NotBeNull();
    }
}
