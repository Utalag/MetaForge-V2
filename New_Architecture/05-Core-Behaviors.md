# Core — Behaviors

> Expression System (PROP-024, PROP-031), Statement System (PROP-031), Constraints, Boundary Analysis, StandardLibraries

**Aktualizace:** PROP-024 (2026-07-04) — Expression hierarchie.
**Aktualizace:** PROP-031 (2026-07-05) — Statement hierarchie, odstraněn ComputedExpression (nahrazen Statement AST).
**Aktualizace:** PROP-031 rozšíření (2026-07-08) — 6 nových statement typů: SwitchStatement, ForEachStatement, TryCatchStatement+CatchClause, UsingStatement, UsingDeclarationStatement, LocalFunctionStatement.
**Aktualizace:** PROP-035 (2026-07-08) — Nové expression typy (LambdaExpression, NewExpression, DefaultExpression, ConversionExpression, AwaitExpression, SwitchExpression, IsPatternExpression, NullCoalescingExpression), NamedArgument v MethodCallExpression, ExpressionKind rozšířen o Await, Switch, IsPattern, NullCoalescing.
**Aktualizace:** PROP-038 (2026-07-08) — Fluent Builder API, MetadataBag, DiagnosticBag+BuildResult\<T\>, TransformPipeline+AttributeReflectionTransform.

---

## Expression System (PROP-024)

Hierarchie výrazů inspirovaná `System.Linq.Expressions`, přizpůsobená pro doménové modelování.
Expressiony se používají uvnitř Statementů (např. `ReturnStatement.Value`, `IfStatement.Condition`, `AssignmentStatement.Value`).

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
    Await,          // PROP-035: await task
    Switch,         // PROP-035: x switch { 1 => "one", _ => "many" }
    IsPattern,      // PROP-035: x is string
    NullCoalescing, // PROP-035: a ?? b
    Computed,       // složený výraz
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

---

## Statement System (PROP-031)

> Typově bezpečná hierarchie statementů nahrazující `ComputedExpression` pro reprezentaci těl metod.
> Expressiony se používají uvnitř Statementů — např. `ReturnStatement.Value: Expression`.

```csharp
// Složka: Src/MetaForge.Core/Elements/Statements/

public abstract class Statement
{
    public abstract StatementKind StatementKind { get; }
}

public enum StatementKind
{
    Block, Return, If, For, While, Assignment, Expression,
    // PROP-031 rozšíření:
    Switch, ForEach, TryCatch, Using, UsingDeclaration, LocalFunction,
}

// Statementy:
// BlockStatement            — { stmt1; stmt2; }
// ReturnStatement           — return X;
// IfStatement               — if (cond) { } else { }
// ForStatement              — for (init; cond; inc) { }
// WhileStatement            — while (cond) { }
// AssignmentStatement       — varName = value;
// ExpressionStatement       — expr; (volání metody apod.)
// SwitchStatement           — switch (expr) { case X: { } default: { } }
// ForEachStatement          — foreach (var item in collection) { }
// TryCatchStatement         — try { } catch (Ex) { } finally { }   + CatchClause
// UsingStatement            — using (resource) { }
// UsingDeclarationStatement — using var x = ...; (C# 8+)
// LocalFunctionStatement    — void Helper() { } uvnitř metody
```

### ExpressionRenderer (v Generators) — aktualizováno

```csharp
// Renderuje Expression a Statement AST → C# kód
public sealed class ExpressionRenderer
{
    public string Render(BlockStatement block);          // tělo metody
    public string Render(BlockStatement block, int indent);
    public string RenderStatement(Statement stmt);       // dispatch
    public string RenderExpression(Expression expr);     // dispatch
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
    StandardLibraryRequirements? Translate(string operationId);
}
```

---

## CoreValidator + Element Factory Metody (PROP-033 — nové)

> **Aktualizace:** PROP-033 (2026-07-05) — validace Core elementů a factory metody pro bezpečnou konstrukci.
> Každá ✅ kombinace z matice = factory metoda. Každá ❌ kombinace = detekce v CoreValidatoru.

### CoreValidator

```csharp
// Složka: Src/MetaForge.Core/Validation/
// Soubory: CoreValidator.cs, ValidationIssue.cs

public static class CoreValidator
{
    /// <summary>Vrátí seznam validačních problémů. Prázdný seznam = validní.</summary>
    public static IReadOnlyList<ValidationIssue> Validate(RootElement element);

    /// <summary>Vyhodí výjimku při prvním problému. Pro fail-fast scénáře.</summary>
    public static void EnsureValid(RootElement element);

    /// <summary>Validátor pro MethodElement (není RootElement).</summary>
    public static IReadOnlyList<ValidationIssue> ValidateMethod(MethodElement method);

    /// <summary>Validátor pro PropertyElement (není RootElement).</summary>
    public static IReadOnlyList<ValidationIssue> ValidateProperty(PropertyElement property);
}

public sealed record ValidationIssue(
    string Code,      // Kód z matice: "C9", "M11", ...
    string Category,  // ConflictingModifiers | InvalidAccess | InvalidType | ...
    string Message    // Lidsky čitelná zpráva
);
```

### Pokryté ❌ řádky matice

| Kód | Popis | Kategorie |
|:---:|-------|-----------|
| C9 | `abstract sealed` | ConflictingModifiers |
| C10 | `abstract static` | ConflictingModifiers |
| C12 | `static record` | ConflictingModifiers |
| A3-A5 | private/protected top-level | InvalidAccess |
| I5 | dědění od sealed typu | InvalidInheritance |
| E5-E6 | nevalidní underlying type enumu | InvalidType |
| P7 | property bez getteru i setteru | MissingRequired |
| T19-T21 | void jako property type | InvalidType |
| M9-M12 | konfliktní modifikátory metod | ConflictingModifiers |

## Fluent Builder API (PROP-038)

> Entry point `TypeModelExtensions.Define(string ns)` pro fluent definici celého modelu.
> Stávající `With*` metody na elementech zůstávají — Buildery jsou tenká facade.

```csharp
// Složka: Src/MetaForge.Core/Builders/

var model = TypeModelExtensions.Define("MyApp.Domain")
    .Class("Order", cls => cls
        .Sealed()
        .Property("Id", TypeModel.Guid, p => p.Init())
        .Metadata(m => m.Set("Docs.Summary", "Customer order")))
    .Enum("Status", e => e.Flags().Member("Active"))
    .Build();  // ModelDefinition

// Buildery: TypeModelBuilder, ClassBuilder, InterfaceBuilder,
//          EnumBuilder, StructBuilder, PropertyBuilder,
//          MethodBuilder, ParameterBuilder
```

## DiagnosticBag + BuildResult\<T\> (PROP-038)

```csharp
// Složka: Src/MetaForge.Core/Diagnostics/

public enum DiagnosticSeverity { Hidden, Info, Warning, Error }

public sealed record ElementPath(string Root, string Element, string? Segment, string? Subsegment);
public sealed record Diagnostic(string Code, string Message, DiagnosticSeverity Severity, ElementPath Location, ...);

public sealed class DiagnosticBag : IDiagnosticCollector
{
    public void Report(Diagnostic diagnostic);
    public bool HasErrors { get; }
    public IReadOnlyList<Diagnostic> ToReadOnly();
    public IReadOnlyList<Diagnostic> Errors();
    public IReadOnlyList<Diagnostic> Warnings();
}

// Monadický wrapper — pipeline se zastaví při chybě
public sealed record BuildResult<T>(T Value, DiagnosticBag Bag)
{
    public bool IsSuccess => !Bag.HasErrors;
    public BuildResult<TOut> Then<TOut>(Func<T, BuildResult<TOut>> next);  // chain
    public BuildResult<TOut> Map<TOut>(Func<T, TOut> mapper);             // map
}

// Reportéry: ConsoleDiagnosticReporter, JsonDiagnosticReporter, InMemoryDiagnosticReporter
```

## TransformPipeline (PROP-038)

```csharp
// Složka: Src/MetaForge.Core/Transforms/

public interface IModelTransform
{
    string Name { get; }
    TypeModel Apply(TypeModel model, TransformContext context);  // čistá funkce
}

public sealed class TransformPipeline
{
    public TransformPipeline Add(IModelTransform transform);
    public TransformPipeline AddIf(Func<TransformContext, bool> predicate, IModelTransform transform);
    public BuildResult<TypeModel> Run(TypeModel model, PipelineOptions? options = null);
}

public sealed class PipelineOptions
{
    public bool FailFast { get; set; } = true;
    public bool EnableReflection { get; set; } = true;
}

// Vestavěný transform: AttributeReflectionTransform (AttributeElement → MetadataBag reflexe)
// Helper: AttributeReflection.MapAttributeToMetadataKey(), ReflectToMetadata()
| B11-B13 | typové chyby ve statementech | StatementTypeError |

### Factory metody na elementech

Factory metody jsou atomické — každá vytváří právě jednu ✅ kombinaci z matice.
Fluent `With*` metody slouží pro vlastnosti bez konfliktů (access modifier, base class, parametry).

Viz [`04-Core-Elements.md`](04-Core-Elements.md) pro kompletní výčet factory metod.

