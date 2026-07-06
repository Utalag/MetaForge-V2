using FluentAssertions;
using MetaForge.Core.DataTypes;
using MetaForge.Core.Elements.Expressions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Statements;
using MetaForge.Core.Elements.Types;
using MetaForge.Generators;

namespace MetaForge.Core.Integration.Tests.Scenarios;

/// <summary>
/// Snapshot testy pro Method varianty s reálnými příklady.
/// Používá AST Statementy pro tělo metody.
/// </summary>
public class MethodSnapshots
{
    private readonly CodeGenerator _generator = new();

    /// <summary>Pythagorova věta — CalculateHypotenuse.</summary>
    [Fact]
    public void PythagoreanTheorem()
    {
        var cls = ClassElement.Static("MathUtils");
        var method = MethodElement.Static("CalculateHypotenuse", TypeModel.Of(DataType.Double))
            .WithParameters(
                new ParameterElement { Name = "a", Type = TypeModel.Of(DataType.Double) },
                new ParameterElement { Name = "b", Type = TypeModel.Of(DataType.Double) })
            .WithBody(new BlockStatement(
                new ReturnStatement
                {
                    Value = new MethodCallExpression("Math.Sqrt",
                        new Expression[]
                        {
                            new BinaryExpression(
                                new BinaryExpression(new MemberAccessExpression("a"), BinaryOperator.Multiply, new MemberAccessExpression("a")),
                                BinaryOperator.Add,
                                new BinaryExpression(new MemberAccessExpression("b"), BinaryOperator.Multiply, new MemberAccessExpression("b")))
                        },
                        TypeModel.Of(DataType.Double))
                }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(PythagoreanTheorem), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("Math.Sqrt");
    }

    /// <summary>Kvadratická rovnice s if/else větvemi.</summary>
    [Fact]
    public void QuadraticEquation()
    {
        var cls = ClassElement.Static("MathUtils");
        var method = MethodElement.Static("SolveQuadratic", TypeModel.String)
            .WithParameters(
                new ParameterElement { Name = "a", Type = TypeModel.Of(DataType.Double) },
                new ParameterElement { Name = "b", Type = TypeModel.Of(DataType.Double) },
                new ParameterElement { Name = "c", Type = TypeModel.Of(DataType.Double) })
            .WithBody(new BlockStatement(
                new AssignmentStatement
                {
                    Variable = "discriminant",
                    Value = new BinaryExpression(
                        new BinaryExpression(new MemberAccessExpression("b"), BinaryOperator.Multiply, new MemberAccessExpression("b")),
                        BinaryOperator.Subtract,
                        new BinaryExpression(
                            new BinaryExpression(new ConstantExpression(4), BinaryOperator.Multiply, new MemberAccessExpression("a")),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("c")))
                },
                new IfStatement
                {
                    Condition = new BinaryExpression(
                        new MemberAccessExpression("discriminant"),
                        BinaryOperator.GreaterThan,
                        new ConstantExpression(0)),
                    TrueBranch = new ReturnStatement
                    {
                        Value = new ConstantExpression("Two real roots")
                    },
                    FalseBranch = new IfStatement
                    {
                        Condition = new BinaryExpression(
                            new MemberAccessExpression("discriminant"),
                            BinaryOperator.Equal,
                            new ConstantExpression(0)),
                        TrueBranch = new ReturnStatement
                        {
                            Value = new ConstantExpression("One real root")
                        },
                        FalseBranch = new ReturnStatement
                        {
                            Value = new ConstantExpression("Complex roots")
                        }
                    }
                }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(QuadraticEquation), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("discriminant").And.Contain("real");
    }

    /// <summary>Faktoriál iterativně — for cyklus.</summary>
    [Fact]
    public void FactorialIterative()
    {
        var cls = ClassElement.Static("MathUtils");
        var method = MethodElement.Static("Factorial", TypeModel.Int32)
            .WithParameter(new ParameterElement { Name = "n", Type = TypeModel.Int32 })
            .WithBody(new BlockStatement(
                new AssignmentStatement { Variable = "result", Value = new ConstantExpression(1) },
                new ForStatement
                {
                    Variable = "i",
                    Start = new ConstantExpression(2),
                    End = new BinaryExpression(
                        new MemberAccessExpression("n"),
                        BinaryOperator.Add,
                        new ConstantExpression(1)),
                    Body = new AssignmentStatement
                    {
                        Variable = "result",
                        Value = new BinaryExpression(
                            new MemberAccessExpression("result"),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("i"))
                    }
                },
                new ReturnStatement { Value = new MemberAccessExpression("result") }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(FactorialIterative), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("for").And.Contain("result");
    }

    /// <summary>Výpočet slevy — if statement s else.</summary>
    [Fact]
    public void CalculateDiscount()
    {
        var cls = ClassElement.Static("Pricing");
        var method = MethodElement.Static("CalculateDiscount", TypeModel.Decimal)
            .WithParameters(
                new ParameterElement { Name = "price", Type = TypeModel.Decimal },
                new ParameterElement { Name = "percent", Type = TypeModel.Decimal })
            .WithBody(new BlockStatement(
                new IfStatement
                {
                    Condition = new BinaryExpression(
                        new MemberAccessExpression("percent"),
                        BinaryOperator.GreaterThan,
                        new ConstantExpression(100)),
                    TrueBranch = new AssignmentStatement
                    {
                        Variable = "percent",
                        Value = new ConstantExpression(100)
                    },
                    FalseBranch = new BlockStatement(
                        new IfStatement
                        {
                            Condition = new BinaryExpression(
                                new MemberAccessExpression("percent"),
                                BinaryOperator.LessThan,
                                new ConstantExpression(0)),
                            TrueBranch = new AssignmentStatement
                            {
                                Variable = "percent",
                                Value = new ConstantExpression(0)
                            }
                        })
                },
                new ReturnStatement
                {
                    Value = new BinaryExpression(
                        new MemberAccessExpression("price"),
                        BinaryOperator.Multiply,
                        new BinaryExpression(
                            new MemberAccessExpression("percent"),
                            BinaryOperator.Divide,
                            new ConstantExpression(100)))
                }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(CalculateDiscount), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("price");
    }

    /// <summary>Validace emailu — string operace.</summary>
    [Fact]
    public void IsValidEmail()
    {
        var cls = ClassElement.Static("Validators");
        var method = MethodElement.Static("IsValidEmail", TypeModel.Bool)
            .WithParameter(new ParameterElement { Name = "email", Type = TypeModel.String })
            .WithBody(new BlockStatement(
                new ReturnStatement
                {
                    Value = new BinaryExpression(
                        new BinaryExpression(
                            new MethodCallExpression("string.IsNullOrWhiteSpace", [new MemberAccessExpression("email")], TypeModel.Bool),
                            BinaryOperator.Equal,
                            new ConstantExpression(false)),
                        BinaryOperator.And,
                        new MethodCallExpression("email.Contains", [new ConstantExpression("@")], TypeModel.Bool))
                }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(IsValidEmail), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("IsValidEmail");
    }

    /// <summary>Celé jméno — string interpolace/skládání.</summary>
    [Fact]
    public void GetFullName()
    {
        var cls = ClassElement.Static("Formatters");
        var method = MethodElement.Static("GetFullName", TypeModel.String)
            .WithParameters(
                new ParameterElement { Name = "first", Type = TypeModel.String },
                new ParameterElement { Name = "last", Type = TypeModel.String })
            .WithBody(new BlockStatement(
                new ReturnStatement
                {
                    Value = new BinaryExpression(
                        new BinaryExpression(
                            new MemberAccessExpression("first"), BinaryOperator.Add, new ConstantExpression(" ")),
                        BinaryOperator.Add,
                        new MemberAccessExpression("last"))
                }));
        cls.Methods.Add(method);

        var result = _generator.Generate(cls);

        SnapshotComparer.Verify("Method", nameof(GetFullName), result.SourceCode);
        SnapshotComparer.AssertValidSyntax(result.SourceCode);
        result.SourceCode.Should().Contain("first").And.Contain("last");
    }
}
