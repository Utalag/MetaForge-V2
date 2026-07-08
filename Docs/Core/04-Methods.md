# Core Methods — Signatura, Parametry, Modifikátory

> Reprezentace metod v Core: signatura, parametry, async, abstraktní, generické metody.

## MethodElement

```csharp
public class MethodElement
{
    public string Name { get; set; } = "";
    public TypeModel ReturnType { get; set; } = TypeModel.Void;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    // Modifikátory
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsExtension { get; set; }

    // Generika
    public List<string> TypeParameters { get; set; } = new();
    public List<GenericConstraint> TypeConstraints { get; set; } = new();

    // Tělo
    public Expression? ExpressionBody { get; set; }
    public BlockStatement? Body { get; set; }

    // Parametry
    public List<ParameterElement> Parameters { get; set; } = new();

    // Metadata
    public List<AttributeElement> Attributes { get; set; } = new();
    public MetadataBag Metadata { get; set; } = new();
}
```

### Statické factory

| Metoda | Výsledek |
|--------|----------|
| `MethodElement.Basic("Execute")` | `public void Execute()` |
| `MethodElement.Static("Parse", TypeModel.String)` | `public static string Parse()` |
| `MethodElement.Async("FetchAsync", TypeModel.Of(...))` | `public async Task<T> FetchAsync()` |
| `MethodElement.Abstract("Process", TypeModel.Void)` | `public abstract void Process()` |
| `MethodElement.Virtual("GetName", TypeModel.String)` | `public virtual string GetName()` |
| `MethodElement.Override("ToString", TypeModel.String)` | `public override string ToString()` |
| `MethodElement.Generic("Find", TypeModel.Of(...), ["T"])` | `public T Find<T>()` |

---

## ParameterElement

```csharp
public class ParameterElement
{
    public string Name { get; set; } = "";
    public TypeModel Type { get; set; } = TypeModel.Object;
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;
}

public enum ParameterModifier { None, Ref, Out, In, Params }
```

---

## MethodBodyKind (návrh — PROP-031 follow-up)

Pro rozlišení typu těla metody:

```csharp
public enum MethodBodyKind
{
    None,        // Abstraktní / interface — žádné tělo
    Structured,  // AST (BlockStatement)
    Text,        // String (generátor ho vloží tak jak je)
    AiBody       // AI-generované tělo (placeholder, doplní AI vrstva)
}
```

> **Status:** 🔵 Plánováno — IDEA-004, follow-up k PROP-031.

---

## Invarianty metod

Vestavěné invarianty z `BuiltInInvariants`:

| Kód | Popis | Severity |
|-----|-------|----------|
| `MF_METHOD_001` | Abstract methods must not have a body | Error |
| `MF_METHOD_002` | Abstract methods cannot be static | Error |
| `MF_METHOD_003` | Abstract methods cannot be private | Error |
| `MF_METHOD_004` | Async methods should return task-like type | Warning |
| `MF_METHOD_005` | Virtual methods cannot be static | Error |
| `MF_METHOD_006` | Override methods require base virtual/abstract | Warning |

---

## Příklady

### C# → Core

```csharp
// C#: public async Task<Customer> GetByIdAsync(Guid id)
var method = MethodElement.Async("GetByIdAsync",
    TypeModel.Of(DataType.Entity)
        .WithCustomName("Task")
        .WithGenericArg(TypeModel.Of(DataType.Entity).WithCustomName("Customer")));

method.Parameters.Add(new ParameterElement
{
    Name = "id",
    Type = TypeModel.Guid
});
```

### Core → C# (generovaný)

```csharp
public async System.Threading.Tasks.Task<Customer> GetByIdAsync(System.Guid id)
{
    // body
}
```
