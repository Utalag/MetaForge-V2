# Core Expressions & AST — Expression Model, Statement System

> Přehled expression a statement modelu, hranice mezi strukturovaným AST a textovým/AI tělem.

## Filozofie

Core rozlišuje tři úrovně reprezentace těl metod:

| Úroveň | Reprezentace | Co je v Core | Kdo řeší zbytek |
|--------|-------------|-------------|-----------------|
| **Structured** | AST (`BlockStatement` + `Expression`) | Plně sémantický model | Generátor renderuje do kódu |
| **Text** | `string` (Body jako text) | Pouze signatura | Generátor vloží text tak jak je |
| **AI Body** | `MethodBodyKind.AiBody` | Placeholder | AI vrstva doplní tělo |

**Hranice:** AST končí u statementů a výrazů. Cokoliv složitějšího (plná těla metod s externími voláními) je `Text` nebo `AiBody`.

---

## Expression System

### Expression (abstract base)

```csharp
public abstract record Expression
{
    public abstract string Kind { get; }
    public abstract ExpressionKind ExpressionKind { get; }
    public TypeModel ResultType { get; init; } = TypeModel.Object;
}
```

### ExpressionKind enum

| Hodnota | C# ekvivalent |
|---------|--------------|
| `Constant` | `42`, `"hello"`, `true` |
| `MemberAccess` | `entity.FirstName` |
| `Binary` | `a + b`, `a > b` |
| `Unary` | `!a`, `-a` |
| `MethodCall` | `string.IsNullOrEmpty(name)` |
| `Lambda` | `(x) => x.FirstName` |
| `New` | `new Customer { Name = "..." }` |
| `Conditional` | `a ? b : c` |
| `Default` | `default(int)` |
| `Conversion` | `(decimal)price` |
| `Computed` | Složený výraz (generic) |
| `Await` | `await task` |
| `Switch` | `x switch { 1 => "one", _ => "many" }` |
| `IsPattern` | `x is string`, `x is not null` |
| `NullCoalescing` | `a ?? b` |

### Konkrétní expression typy

| Typ | Klíčové vlastnosti |
|-----|-------------------|
| `ConstantExpression` | `Value` (object?) |
| `MemberAccessExpression` | `MemberPath` (string) |
| `BinaryExpression` | `Left`, `Operator` (BinaryOperator), `Right` |
| `UnaryExpression` | `Operator` (UnaryOperator), `Operand` |
| `MethodCallExpression` | `MethodName`, `Arguments`, `ArgumentNames` |
| `LambdaExpression` | `ParameterNames`, `Body`, `IsAsync` |
| `NewExpression` | `TypeName`, `ConstructorArguments`, `MemberBindings` |
| `ConditionalExpression` | `Condition`, `WhenTrue`, `WhenFalse` |
| `DefaultExpression` | `TargetType` |
| `ConversionExpression` | `TargetType`, `Operand`, `IsExplicit` |
| `AwaitExpression` | `Operand` |
| `SwitchExpression` | `Selector`, `Arms` (SwitchArm[]) |
| `IsPatternExpression` | `Operand`, `PatternKind`, `IsNegated` |
| `NullCoalescingExpression` | `Left`, `Right` |

### BinaryOperator (15 hodnot)

`Add`, `Subtract`, `Multiply`, `Divide`, `Modulo`, `Equal`, `NotEqual`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `And`, `Or`, `Concat`, `NullCoalesce`

### UnaryOperator (5 hodnot)

`Not`, `Negate`, `BitwiseNot`, `Increment`, `Decrement`

### PatternKind

`Type`, `Null`, `Constant`

---

## Statement System

### Statement (abstract base)

```csharp
public abstract record Statement
{
    public abstract StatementKind StatementKind { get; }
}
```

### StatementKind enum (13 hodnot)

| Hodnota | C# ekvivalent |
|---------|--------------|
| `Block` | `{ ... }` |
| `Return` | `return X;` |
| `If` | `if (cond) { } else { }` |
| `For` | `for (init; cond; inc) { }` |
| `While` | `while (cond) { }` |
| `Assignment` | `varName = value;` |
| `Expression` | Výraz jako statement |
| `Switch` | `switch (expr) { case X: ... }` |
| `ForEach` | `foreach (var item in collection) { }` |
| `TryCatch` | `try { } catch (Ex) { } finally { }` |
| `Using` | `using (resource) { }` |
| `UsingDeclaration` | `using var x = ...;` |
| `LocalFunction` | `void Helper() { }` |

### Konkrétní statement typy

| Typ | Klíčové vlastnosti |
|-----|-------------------|
| `BlockStatement` | `Statements` (List) |
| `ReturnStatement` | `Value` (Expression?) |
| `IfStatement` | `Condition`, `TrueBranch?`, `FalseBranch?` |
| `ForStatement` | `Variable`, `Start`, `End`, `Body?` |
| `WhileStatement` | `Condition`, `Body` |
| `AssignmentStatement` | `Variable`, `Value` |
| `ExpressionStatement` | `Expr` |
| `SwitchStatement` | `Selector`, `Cases`, `DefaultCase?` |
| `ForEachStatement` | `VariableName`, `Collection`, `Body?` |
| `TryCatchStatement` | `TryBody`, `Catches`, `FinallyBody?` |
| `UsingStatement` | `ResourceDeclaration?`, `Body?` |
| `UsingDeclarationStatement` | `VariableName`, `Initializer` |
| `LocalFunctionStatement` | `Function` (MethodElement) |

---

## Příklad: AST → C# kód

```csharp
// Core AST:
var body = new BlockStatement();
body.Statements.Add(new IfStatement
{
    Condition = new BinaryExpression
    {
        Left = new MemberAccessExpression { MemberPath = "value" },
        Operator = BinaryOperator.GreaterThan,
        Right = new ConstantExpression { Value = 0 }
    },
    TrueBranch = new ReturnStatement
    {
        Value = new MemberAccessExpression { MemberPath = "value" }
    }
});

// Generovaný C#:
// {
//     if (value > 0)
//         return value;
// }
```

---

## Stav podpory: ✅ Supported (15 expression typů, 13 statement typů)
