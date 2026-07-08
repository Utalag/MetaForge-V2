// ---------------------------------------------------------------------------
// MetaForge.Core.Tests — ReflectionBasedInvariantEvaluatorTests
// Integration tests for the default invariant evaluator against real elements.
// PROPOSAL: PROP-036 — Core Specification Layer
// ---------------------------------------------------------------------------

using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;
using MetaForge.Core.Specifications;
using static MetaForge.Core.Specifications.InvariantExpressionBuilder;

namespace MetaForge.Core.Tests.Specifications;

public class ReflectionBasedInvariantEvaluatorTests
{
    private readonly ReflectionBasedInvariantEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_AbstractMethod_WithBody_ReturnsViolation()
    {
        // Arrange
        var method = new MethodElement
        {
            Name = "DoSomething",
            IsAbstract = true,
            Body = new MetaForge.Core.Elements.Statements.BlockStatement()
        };

        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotHaveBody };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(method, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_METHOD_001", result.Violations[0].Code);
        Assert.Equal(InvariantSeverity.Error, result.Violations[0].Severity);
    }

    [Fact]
    public void Evaluate_AbstractMethod_WithoutBody_NoViolation()
    {
        // Arrange
        var method = new MethodElement
        {
            Name = "DoSomething",
            IsAbstract = true,
            Body = null
        };

        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotHaveBody };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(method, context, invariants);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_NonAbstractMethod_WithBody_NoViolation()
    {
        // Arrange — When condition not met (IsAbstract = false)
        var method = new MethodElement
        {
            Name = "DoSomething",
            IsAbstract = false,
            Body = new MetaForge.Core.Elements.Statements.BlockStatement()
        };

        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotHaveBody };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(method, context, invariants);

        // Assert — invariant does not apply (When condition not met)
        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_AbstractMethod_Static_ReturnsViolation()
    {
        // Arrange
        var method = new MethodElement
        {
            Name = "DoSomething",
            IsAbstract = true,
            IsStatic = true
        };

        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotBeStatic };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(method, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_METHOD_002", result.Violations[0].Code);
    }

    [Fact]
    public void Evaluate_AbstractSealedClass_ReturnsViolation()
    {
        // Arrange
        var cls = new ClassElement
        {
            Name = "BadClass",
            IsAbstract = true,
            IsSealed = true
        };

        var invariants = new[] { BuiltInInvariants.Class_AbstractSealedConflict };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(cls, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_CLASS_001", result.Violations[0].Code);
    }

    [Fact]
    public void Evaluate_ValidClass_NoViolations()
    {
        // Arrange
        var cls = new ClassElement
        {
            Name = "GoodClass",
            IsAbstract = false,
            IsSealed = true,
            IsStatic = false
        };

        var invariants = new[]
        {
            BuiltInInvariants.Class_AbstractSealedConflict,
            BuiltInInvariants.Class_StaticAbstractConflict,
            BuiltInInvariants.Class_MustHaveName
        };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(cls, context, invariants);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_Class_EmptyName_ReturnsViolation()
    {
        // Arrange
        var cls = new ClassElement { Name = "" };

        var invariants = new[] { BuiltInInvariants.Class_MustHaveName };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(cls, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_CLASS_004", result.Violations[0].Code);
    }

    [Fact]
    public void Evaluate_StaticProperty_Required_ReturnsViolation()
    {
        // Arrange
        var prop = new PropertyElement
        {
            Name = "BadProp",
            IsStatic = true,
            IsRequired = true
        };

        var invariants = new[] { BuiltInInvariants.Property_StaticCannotBeRequired };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(prop, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_PROP_001", result.Violations[0].Code);
    }

    [Fact]
    public void Evaluate_RequiredProperty_WithoutSetter_ReturnsViolation()
    {
        // Arrange
        var prop = new PropertyElement
        {
            Name = "RequiredProp",
            IsRequired = true,
            HasSetter = false,
            IsInitOnly = false
        };

        var invariants = new[] { BuiltInInvariants.Property_RequiredNeedsSetter };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(prop, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Violations);
        Assert.Equal("MF_PROP_002", result.Violations[0].Code);
    }

    [Fact]
    public void Evaluate_RequiredProperty_WithInitOnly_NoViolation()
    {
        // Arrange
        var prop = new PropertyElement
        {
            Name = "RequiredProp",
            IsRequired = true,
            HasSetter = false,
            IsInitOnly = true
        };

        var invariants = new[] { BuiltInInvariants.Property_RequiredNeedsSetter };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(prop, context, invariants);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_InvariantForDifferentTargetKind_Skipped()
    {
        // Arrange — Method invariant against a ClassElement should be skipped
        var cls = new ClassElement { Name = "Test" };
        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotHaveBody };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(cls, context, invariants);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1, result.TotalEvaluated);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_MultipleInvariants_ReportsAllViolations()
    {
        // Arrange
        var method = new MethodElement
        {
            Name = "DoSomething",
            IsAbstract = true,
            IsStatic = true,
            Body = new MetaForge.Core.Elements.Statements.BlockStatement()
        };

        var invariants = new[]
        {
            BuiltInInvariants.Method_AbstractCannotHaveBody,
            BuiltInInvariants.Method_AbstractCannotBeStatic,
        };
        var context = InvariantEvaluationContext.Local();

        // Act
        var result = _evaluator.Evaluate(method, context, invariants);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Violations.Count);
        Assert.Contains(result.Violations, v => v.Code == "MF_METHOD_001");
        Assert.Contains(result.Violations, v => v.Code == "MF_METHOD_002");
    }

    [Fact]
    public void EvaluationResult_TracksTimings()
    {
        var method = new MethodElement { Name = "Test" };
        var invariants = new[] { BuiltInInvariants.Method_AbstractCannotHaveBody };
        var context = InvariantEvaluationContext.Local();

        var result = _evaluator.Evaluate(method, context, invariants);

        Assert.Equal(1, result.TotalEvaluated);
        Assert.True(result.EvaluationTime >= TimeSpan.Zero);
    }
}
