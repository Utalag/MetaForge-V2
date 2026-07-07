using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Generators.Tests;

/// <summary>
/// Testy renderování generických typových parametrů a constraints
/// pro ClassElement, InterfaceElement, StructElement a MethodElement.
/// </summary>
public class GenericsGeneratorTests
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void Generate_ClassWithTypeParameter_ContainsAngleBrackets()
    {
        var cls = ClassElement.Basic("Repository")
            .WithTypeParameter(TypeParameterElement.Of("T").WithConstraint(GenericConstraint.Class()));

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("class Repository<T>");
        result.SourceCode.Should().Contain("where T : class");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_ClassWithMultipleConstraints_RendersAllConstraints()
    {
        var cls = ClassElement.Basic("Cache")
            .WithTypeParameter(TypeParameterElement.Of("TKey").WithConstraint(GenericConstraint.NotNull()))
            .WithTypeParameter(TypeParameterElement.Of("TValue").WithConstraint(GenericConstraint.NewConstructor()));

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("class Cache<TKey, TValue>");
        result.SourceCode.Should().Contain("where TKey : notnull");
        result.SourceCode.Should().Contain("where TValue : new()");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_InterfaceWithTypeParameter_ContainsAngleBrackets()
    {
        var iface = InterfaceElement.Basic("IRepository")
            .WithTypeParameter(TypeParameterElement.Of("T").WithConstraint(GenericConstraint.BaseType("Entity")));

        var result = _generator.Generate(iface);

        result.SourceCode.Should().Contain("interface IRepository<T>");
        result.SourceCode.Should().Contain("where T : Entity");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_StructWithTypeParameter_ContainsAngleBrackets()
    {
        var str = StructElement.Basic("Pair")
            .WithTypeParameter(TypeParameterElement.Of("T"));

        var result = _generator.Generate(str);

        result.SourceCode.Should().Contain("struct Pair<T>");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_MethodWithTypeParameter_ContainsAngleBracketsAndConstraint()
    {
        var cls = ClassElement.Basic("Factory");
        cls.WithMethod(MethodElement.Basic("Create")
            .WithTypeParameter(TypeParameterElement.Of("T").WithConstraint(GenericConstraint.NewConstructor()))
            .WithBody(new MetaForge.Core.Elements.Statements.BlockStatement(
                new MetaForge.Core.Elements.Statements.ReturnStatement
                {
                    Value = new MetaForge.Core.Elements.Expressions.NewExpression("T")
                })));
        cls.Methods[0].ReturnType = TypeModel.Object.WithCustomName("T");

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("Create<T>()");
        result.SourceCode.Should().Contain("where T : new()");
        SyntaxValidator.IsValid(result.SourceCode, out var diagnostics).Should().BeTrue(diagnostics);
    }

    [Fact]
    public void Generate_ClassWithoutTypeParameters_NoAngleBrackets()
    {
        var cls = ClassElement.Basic("Customer");
        var result = _generator.Generate(cls);

        result.SourceCode.Should().NotContain("<");
    }
}
