# Core — Behaviors

> Expression System (PROP-024), Constraints, Boundary Analysis, StandardLibraries

**Aktualizace:** PROP-024 (2026-07-04) — Expression hierarchie (11 druhů), ExpressionKind enum.

---

## Expression System (PROP-024)

Hierarchie výrazů inspirovaná `System.Linq.Expressions`, přizpůsobená pro doménové modelování.

```csharp
// Složka: Src/MetaForge.Core/Elements/Expressions/

public abstract class Expression
{
    public abstract string Kind { get; }
    public abstract ExpressionKind ExpressionKind { get; }
    public TypeModel ResultType { get; init; } = TypeModel.Object;
}

public enum ExpressionKind
{
    Constant,       // 42, "hello", true
    MemberAccess,   // entity.FirstName
    Binary,         // a + b, a > b, a AND b
    Unary,          // !a, -a
    MethodCall,     // string.IsNullOrEmpty(name)
    Lambda,         // (x) => x.FirstName
    New,            // new Customer { Name = "..." }
    Conditional,    // a ? b : c
    Default,        // default(int)
    Conversion,     // (decimal)price
    Computed,       // ComputedOperation wrapper
}
```

### Konkrétní výrazy

```csharp
// Konstantní hodnota
public sealed record ConstantExpression(object? Value) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Constant;
}

// Přístup k členu (tečková notace)
public sealed record MemberAccessExpression(string MemberPath) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.MemberAccess;
}

// Binární operace (15 operátorů)
public sealed record BinaryExpression(
    Expression Left, BinaryOperator Op, Expression Right) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Binary;
}

public enum BinaryOperator
{
    Add, Subtract, Multiply, Divide, Modulo,
    Equal, NotEqual, GreaterThan, LessThan,
    GreaterThanOrEqual, LessThanOrEqual,
    And, Or, Concat, NullCoalesce,
}

// Unární operace (5 operátorů)
public sealed record UnaryExpression(UnaryOperator Operator, Expression Operand) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Unary;
}

public enum UnaryOperator { Not, Negate, BitwiseNot, Increment, Decrement }

// Podmíněný výraz
public sealed record ConditionalExpression(
    Expression Condition, Expression WhenTrue, Expression WhenFalse) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Conditional;
}

// Volání metody
public sealed record MethodCallExpression(
    string MethodName, IReadOnlyList<Expression> Arguments) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.MethodCall;
}

// Computed wrapper — pro ForgeBlock operace
public sealed record ComputedExpression(
    ComputedOperation Operation, IReadOnlyList<Expression> Operands) : Expression
{
    public override ExpressionKind ExpressionKind => ExpressionKind.Computed;
}

public sealed record ComputedOperation(
    string OperationId, string DisplayName, string? Description = null);
```

### Použití

```csharp
// Příklad: Price >= 0 AND Price <= 1000000
var expr = new BinaryExpression(
    new BinaryExpression(
        new MemberAccessExpression("Price"),
        BinaryOperator.GreaterThanOrEqual,
        new ConstantExpression(0)
    ),
    BinaryOperator.And,
    new BinaryExpression(
        new MemberAccessExpression("Price"),
        BinaryOperator.LessThanOrEqual,
        new ConstantExpression(1000000)
    )
);
```

### ExpressionRenderer (v Generators)

```csharp
// Renderuje Expression → C# kód
public sealed class ExpressionRenderer
{
    public string Render(ComputedExpression expr);
    public string Render(ComputedExpression expr, int indent);
}
```

---

## IConstraintInferencer

```csharp
// Složka: Src/MetaForge.Core/Inference/

public interface IConstraintInferencer
{
    IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
```

Implementace: `RuleBasedConstraintInferencer` (deterministická), `AiConstraintInferencer` (AI-assisted — v MetaForge.Ai).

---

## IStandardLibraryTranslator

```csharp
public interface IStandardLibraryTranslator
{
    string OperationId { get; }
    StandardLibraryRequirements? Translate(ComputedOperation operation);
}
```
