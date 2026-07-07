using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Generators.Tests;

/// <summary>
/// Testy generování delegate/event/operator/extension method konstruktů.
/// </summary>
public class DelegateEventOperatorGeneratorTests
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void Generate_Delegate_ProducesValidSyntax()
    {
        var del = DelegateElement.Basic("EventHandler")
            .WithParameter(new ParameterElement { Name = "sender", Type = TypeModel.Object })
            .WithParameter(new ParameterElement { Name = "e", Type = TypeModel.Object.WithCustomName("EventArgs") });

        var result = _generator.Generate(del);

        result.SourceCode.Should().Contain("public delegate void EventHandler(object sender, EventArgs e);");
        SyntaxValidator.IsValid($"public class Wrapper {{ {result.SourceCode} }}", out var diagnostics)
            .Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_GenericDelegate_ContainsTypeParameter()
    {
        var del = DelegateElement.Basic("Factory")
            .WithTypeParameter(TypeParameterElement.Of("T"))
            .WithParameter(new ParameterElement { Name = "seed", Type = TypeModel.Object.WithCustomName("T") });
        del.ReturnType = TypeModel.Object.WithCustomName("T");

        var result = _generator.Generate(del);

        result.SourceCode.Should().Contain("delegate T Factory<T>(T seed);");
    }

    [Fact]
    public void Generate_ClassWithEvent_ContainsEventDeclaration()
    {
        var cls = ClassElement.Basic("OrderService")
            .WithEvent(EventElement.Basic("OrderPlaced", "EventHandler"));

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("public event EventHandler OrderPlaced;");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_ClassWithOperator_ContainsOperatorDeclaration()
    {
        var cls = ClassElement.Basic("Money");
        cls.WithProperty(new PropertyElement { Name = "Amount", Type = TypeModel.Decimal });
        cls.WithOperator(OperatorElement.Binary(
            OperatorKind.Add,
            TypeModel.Object.WithCustomName("Money"),
            new ParameterElement { Name = "a", Type = TypeModel.Object.WithCustomName("Money") },
            new ParameterElement { Name = "b", Type = TypeModel.Object.WithCustomName("Money") })
            .WithBody(new BlockStatement(new ReturnStatement
            {
                Value = new MetaForge.Core.Elements.Expressions.MemberAccessExpression("a")
            })));

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("public static Money operator +(Money a, Money b)");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_StaticClassWithExtensionMethod_ContainsThisModifier()
    {
        var cls = ClassElement.Static("StringExtensions");
        cls.WithMethod(MethodElement.Basic("IsNullOrEmpty")
            .AsExtensionMethod()
            .WithParameters(new ParameterElement { Name = "value", Type = TypeModel.String })
            .WithBody(new BlockStatement(new ReturnStatement
            {
                Value = new MetaForge.Core.Elements.Expressions.ConstantExpression(true)
            })));
        cls.Methods[0].ReturnType = TypeModel.Bool;

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("IsNullOrEmpty(this string value)");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }
}
