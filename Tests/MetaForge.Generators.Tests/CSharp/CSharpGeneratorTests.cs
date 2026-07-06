using FluentAssertions;
using MetaForge.Core.Abstractions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Generators.Tests;

public class CodeGeneratorTests
{
    private readonly CodeGenerator _generator = new();

    [Fact]
    public void Generate_ClassElement_ContainsPublicClass()
    {
        var cls = new ClassElement { Name = "Customer" };
        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("public class Customer");
        result.FileName.Should().Be("Customer.cs");
    }

    [Fact]
    public void Generate_ClassWithProperty_ContainsPropertyDeclaration()
    {
        var cls = new ClassElement { Name = "Customer" };
        cls.Properties.Add(new PropertyElement
        {
            Name = "FirstName",
            Type = TypeModel.String,
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("string FirstName");
        result.SourceCode.Should().Contain("get;");
    }

    [Fact]
    public void Generate_ClassWithMethod_ContainsMethodDeclaration()
    {
        var cls = new ClassElement { Name = "Customer" };
        cls.Methods.Add(new MethodElement
        {
            Name = "GetFullName",
            ReturnType = TypeModel.String,
            Body = new BlockStatement(
                new ReturnStatement
                {
                    Value = new ConstantExpression("test")
                }
            ),
        });

        var result = _generator.Generate(cls);

        result.SourceCode.Should().Contain("string GetFullName()");
        result.SourceCode.Should().Contain("return \"test\";");
    }

    [Fact]
    public void Generate_EmptyName_ReturnsError()
    {
        var cls = new ClassElement { Name = "" };
        var result = _generator.Generate(cls);

        result.Diagnostics.Should().NotBeNull();
        result.Diagnostics!.Should().Contain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Generate_Enum_ContainsEnumDeclaration()
    {
        var enm = new EnumElement { Name = "Status" };
        enm.Members.Add(new EnumMemberElement { Name = "Active" });
        enm.Members.Add(new EnumMemberElement { Name = "Inactive" });

        var result = _generator.Generate(enm);

        result.SourceCode.Should().Contain("public enum Status");
        result.SourceCode.Should().Contain("Active");
        result.SourceCode.Should().Contain("Inactive");
    }

    [Fact]
    public void Generate_Interface_ContainsInterfaceDeclaration()
    {
        var iface = new InterfaceElement { Name = "IRepository" };
        iface.Methods.Add(new MethodElement
        {
            Name = "GetById",
            ReturnType = TypeModel.Object,
        });

        var result = _generator.Generate(iface);

        result.SourceCode.Should().Contain("interface IRepository");
        result.SourceCode.Should().Contain("GetById");
    }

    [Fact]
    public void Generate_Struct_ContainsStructDeclaration()
    {
        var str = new StructElement { Name = "Point" };
        str.Properties.Add(new PropertyElement { Name = "X", Type = TypeModel.Int32 });

        var result = _generator.Generate(str);

        result.SourceCode.Should().Contain("struct Point");
        result.SourceCode.Should().Contain("int X");
    }

    [Fact]
    public void Generated_Code_IsValidCSharpSyntax()
    {
        // Arrange
        var cls = new ClassElement { Name = "Customer" };
        cls.Properties.Add(new PropertyElement { Name = "Id", Type = TypeModel.Int32 });
        cls.Properties.Add(new PropertyElement { Name = "Name", Type = TypeModel.String });

        var iface = new InterfaceElement { Name = "IRepository" };
        iface.Methods.Add(new MethodElement { Name = "GetById", ReturnType = TypeModel.Int32 });

        var enm = new EnumElement { Name = "Status" };
        enm.Members.Add(new EnumMemberElement { Name = "Active" });
        enm.Members.Add(new EnumMemberElement { Name = "Inactive" });

        var str = new StructElement { Name = "Point" };
        str.Properties.Add(new PropertyElement { Name = "X", Type = TypeModel.Int32 });

        // Act & Assert
        foreach (var result in new[]
        {
            _generator.Generate(cls),
            _generator.Generate(iface),
            _generator.Generate(enm),
            _generator.Generate(str)
        })
        {
            var isValid = SyntaxValidator.IsValid(result.SourceCode, out var diagnostics);
            isValid.Should().BeTrue($"syntax error in {result.FileName}:{Environment.NewLine}{diagnostics}");
        }
    }
}
