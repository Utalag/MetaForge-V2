# PROP-031: Core — Statement System a upgrade Expression pro těla metod

> **Stav:** ✅ Dokončeno
> **Datum:** 2026-07-05
> **Autor:** Copilot (C# Implementer)
> **Návaznost:** PROP-002 (Core base), PROP-024 (Expression System)

---

## Cíl

Zavést **Statement hierarchii** do Core vrstvy jako typově bezpečnou reprezentaci těl metod a konstruktorů. Nahradit současný `MethodElement.Body: string?` (raw C# text) plnohodnotným AST.

Současně rozšířit ExpressionRenderer v Generators vrstvě o renderování statementů do C# kódu.

## Důvod

Aktuální `MethodElement.Body` je `string?` — raw C# kód. To znamená:
- ❌ Žádná typová kontrola — cokoliv lze vložit
- ❌ Nelze validovat strukturu (např. return v async musí mít `Task<T>`)
- ❌ AI nemůže sestavovat logiku programově (musí generovat string)
- ❌ Nelze transformovat / analyzovat stromově
- ❌ Integrační testy metod nemohou testovat strukturu — jen porovnávají stringy

Původní kód ve `For_Inspiration/Core/Elements/Expressions/Ast/` už měl propracovaný AST s `IAstNode`, visitor patternem a 14 nody. Nová implementace jde cestou **typově bezpečné Statement hierarchie** (Varianta B) — konzistentní se současným `Expression` systémem.

## Statement hierarchie

### Nové třídy v `Src/MetaForge.Core/Elements/Statements/`

```
Statements/
├── Statement.cs               ← abstract base (podobné jako Expression)
├── BlockStatement.cs          ← List<Statement> — složený blok
├── ReturnStatement.cs         ← Expression? Value
├── IfStatement.cs             ← Expression Condition, Statement? TrueBranch, Statement? FalseBranch
├── ForStatement.cs            ← Variable, Expression Start, Expression End, Statement? Body
├── WhileStatement.cs          ← Expression Condition, Statement Body
├── AssignmentStatement.cs     ← string Variable, Expression Value
└── ExpressionStatement.cs     ← Expression Expr (pro volání metod bez návratu)
```

### Návrh Statement base class

```csharp
namespace MetaForge.Core.Elements.Statements;

/// <summary>
/// Abstraktní bázová třída pro všechny statementy (příkazy) v těle metod a konstruktorů.
/// Statementy reprezentují imperativní logiku — přiřazení, podmínky, cykly, návratové hodnoty.
/// </summary>
public abstract class Statement
{
    /// <summary>Druh statementu (pro dispatch v rendereru).</summary>
    public abstract StatementKind StatementKind { get; }
}

public enum StatementKind
{
    Block,          // { ... }
    Return,         // return X;
    If,             // if (cond) { } else { }
    For,            // for (init; cond; inc) { }
    While,          // while (cond) { }
    Assignment,     // varName = value;
    Expression,     // volání metody, inkrementace
}
```

### Návrh BlockStatement

```csharp
public sealed class BlockStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Block;
    public List<Statement> Statements { get; } = [];
    
    public BlockStatement() { }
    public BlockStatement(params Statement[] statements) => Statements.AddRange(statements);
}
```

### Návrh ReturnStatement

```csharp
/// <summary>return X; — Value je null pro `return;` (void metoda).</summary>
public sealed class ReturnStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Return;
    public Expression? Value { get; init; }
}
```

### Návrh IfStatement

```csharp
/// <summary>if (Condition) TrueBranch else FalseBranch.</summary>
public sealed class IfStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.If;
    public Expression Condition { get; init; } = default!;
    public Statement? TrueBranch { get; init; }
    public Statement? FalseBranch { get; init; }  // else blok
}
```

### Návrh ForStatement

```csharp
/// <summary>for (Variable = Start; Variable < End; Variable++) Body.</summary>
public sealed class ForStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.For;
    public string Variable { get; init; } = string.Empty;
    public Expression Start { get; init; } = default!;
    public Expression End { get; init; } = default!;
    public Statement? Body { get; init; }
}
```

### Návrh WhileStatement

```csharp
/// <summary>while (Condition) Body.</summary>
public sealed class WhileStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.While;
    public Expression Condition { get; init; } = default!;
    public Statement Body { get; init; } = default!;
}
```

### Návrh AssignmentStatement

```csharp
/// <summary>varName = Value;</summary>
public sealed class AssignmentStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Assignment;
    public string Variable { get; init; } = string.Empty;
    public Expression Value { get; init; } = default!;
}
```

### Návrh ExpressionStatement

```csharp
/// <summary>Výraz jako statement — např. volání metody, `i++`.</summary>
public sealed class ExpressionStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Expression;
    public Expression Expr { get; init; } = default!;
}
```

---

## Změna v MethodElement

```csharp
// PŘED (aktuální):
public string? Body { get; set; }

// PO (nové):
public BlockStatement? Body { get; set; }
```

Stejná změna se aplikuje na nový `ConstructorElement` (budoucí PROP).

---

## Rozšíření ExpressionRendereru

Soubor: `Src/MetaForge.Generators/ExpressionRenderer.cs`

Přidat metodu `RenderStatement(Statement stmt)` s typovým dispatchem:

```csharp
public string RenderStatement(Statement stmt) => stmt switch
{
    BlockStatement block => RenderBlock(block),
    ReturnStatement ret => $"return {RenderExpression(ret.Value)};",
    IfStatement ifs => RenderIf(ifs),
    ForStatement forS => RenderFor(forS),
    WhileStatement whileS => RenderWhile(whileS),
    AssignmentStatement assign => $"{assign.Variable} = {RenderExpression(assign.Value)};",
    ExpressionStatement expr => $"{RenderExpression(expr.Expr)};",
    _ => $"/* unknown statement: {stmt.StatementKind} */"
};
```

---

## Příklad: Pythagorova věta jako AST

```csharp
var method = new MethodElement
{
    Name = "CalculateHypotenuse",
    ReturnType = TypeModel.Double,
    IsStatic = true,
    Parameters = { new ParameterElement("a", TypeModel.Double), new ParameterElement("b", TypeModel.Double) },
    Body = new BlockStatement(
        new ReturnStatement(
            new MethodCallExpression(
                "Math.Sqrt",
                new[]
                {
                    new BinaryExpression(
                        new BinaryExpression(new MemberAccessExpression("a"), BinaryOperator.Multiply, new MemberAccessExpression("a")),
                        BinaryOperator.Add,
                        new BinaryExpression(new MemberAccessExpression("b"), BinaryOperator.Multiply, new MemberAccessExpression("b"))
                    )
                },
                TypeModel.Double
            )
        )
    )
};
```

Vyrenderuje:
```csharp
public static double CalculateHypotenuse(double a, double b)
{
    return Math.Sqrt(a * a + b * b);
}
```

---

## Validace (budoucí rozšíření)

S Statement hierarchií lze přidat validaci:

- `ReturnStatement` v `async Task` metodě musí vracet `Task<T>` nebo výraz kompatibilní s `T`
- `ReturnStatement` v `void` metodě musí mít `Value == null`
- `IfStatement` musí mít `Condition.ResultType.BaseType == DataType.Bool`
- Každá větev toku musí končit `ReturnStatement` (pro non-void metody)

---

## Dotčené soubory

### Nové (Core)
| Soubor | Umístění |
|--------|----------|
| `Statement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `StatementKind.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `BlockStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `ReturnStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `IfStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `ForStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `WhileStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `AssignmentStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |
| `ExpressionStatement.cs` | `Src/MetaForge.Core/Elements/Statements/` |

### Upravené (Core)
| Soubor | Změna |
|--------|-------|
| `MethodElement.cs` | `Body: string?` → `Body: BlockStatement?` |

### Upravené (Generators)
| Soubor | Změna |
|--------|-------|
| `ExpressionRenderer.cs` | Přidat `RenderStatement()` + renderování všech statement typů |
| `CodeGenerator.cs` | Upravit generování metod — volat `RenderStatement(method.Body)` místo vkládání `method.Body` stringu |

---

## Odhad

| Fáze | Práce | Odhad |
|------|-------|------:|
| Návrh Statement tříd | 9 souborů | 1,5 dne |
| Úprava MethodElement | 1 soubor | 0,25 dne |
| Rozšíření ExpressionRendereru | 1 soubor + 7 render metod | 1 den |
| Úprava CodeGeneratoru | 1 soubor | 0,5 dne |
| Unit testy — Statementy | Core.Tests | 1 den |
| Unit testy — Renderer | Generators.Tests | 1 den |
| **Celkem** | | **~5,25 dne** |

---

## Legenda

- Status: 📝 Navrženo
- Vrstva: Core + Generators
- Návaznost: PROP-002, PROP-024, PROP-032 (integrační testy)
- Priorita: 🟡 Vysoká — blokuje PROP-032 (integrační testy metod)
