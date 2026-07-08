# Core — Code Elements

> Class, Interface, Enum, Struct, Property, Method, Parameter

---

## ClassElement

```csharp
public sealed class ClassElement : RootElement
{
    public override string Kind => "class";
    public string? BaseClassName { get; set; }
    public List<string> ImplementedInterfaces { get; } = new();
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }

    // PROP-035: C#-first
    public List<string> TypeParameters { get; init; } = [];           // <T, TKey>
    public List<GenericConstraint> TypeConstraints { get; init; } = []; // where T : class, new()
    public List<ParameterElement>? PrimaryConstructorParameters { get; set; } // Point(int X, int Y)

    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);

    // Factory metody (PROP-035 rozšířeno)
    public static ClassElement PrimaryRecord(string name, params ParameterElement[] parameters);
    public static ClassElement Generic(string name, string[] typeParameters, GenericConstraint[]? constraints);
}
```

## InterfaceElement

```csharp
public sealed class InterfaceElement : RootElement
{
    public override string Kind => "interface";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

## EnumElement

```csharp
public sealed class EnumElement : RootElement
{
    public override string Kind => "enum";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public DataType UnderlyingType { get; set; } = DataType.Int32;
    public bool IsFlags { get; set; }
    public List<EnumMemberElement> Members { get; } = new();

    public override int TotalCoin =>
        Coin + Members.Sum(m => m.Coin);
}

public sealed class EnumMemberElement
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public List<AttributeElement> Attributes { get; } = new();

    public int Coin { get; set; } = 1;
}
```

## StructElement

```csharp
public sealed class StructElement : RootElement
{
    public override string Kind => "struct";
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsReadOnly { get; set; }
    public bool IsRecord { get; set; }
    public List<PropertyElement> Properties { get; } = new();
    public List<MethodElement> Methods { get; } = new();

    public override int TotalCoin =>
        Coin + Properties.Sum(p => p.Coin) + Methods.Sum(m => m.TotalCoin);
}
```

## PropertyElement

```csharp
public sealed class PropertyElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel Type { get; set; } = TypeModel.Object;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool HasGetter { get; set; } = true;
    public bool HasSetter { get; set; } = true;
    public bool IsInitOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool IsStatic { get; set; }
    public string? DefaultValue { get; set; }
    public MetadataBag Metadata { get; init; } = new();   // PROP-038

    public int Coin { get; set; } = 2;
}
```

## MethodElement

```csharp
public sealed class MethodElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel ReturnType { get; set; } = TypeModel.Void;
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
    public bool IsStatic { get; set; }
    public bool IsAsync { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }

    // PROP-035: C#-first
    public bool IsExtension { get; set; }                           // this TypeName
    public List<string> TypeParameters { get; init; } = [];         // <T>
    public List<GenericConstraint> TypeConstraints { get; init; } = []; // where T : struct
    public Expression? ExpressionBody { get; set; }                 // => x * 2 (null = block body)

    public List<ParameterElement> Parameters { get; } = new();
    public List<AttributeElement> Attributes { get; } = new();
    public BlockStatement? Body { get; set; }
    public MetadataBag Metadata { get; init; } = new();   // PROP-038

    public int Coin { get; set; } = 5;
    public int TotalCoin => Coin + Parameters.Sum(p => p.Coin);

    // Factory: generická metoda (PROP-035)
    public static MethodElement Generic(string name, TypeModel returnType, string[] typeParameters, GenericConstraint[]? constraints);
}
```

## ParameterElement

```csharp
public sealed class ParameterElement
{
    public string Name { get; set; } = string.Empty;
    public TypeModel Type { get; set; } = TypeModel.Object;
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public ParameterModifier Modifier { get; set; } = ParameterModifier.None;

    public int Coin { get; set; } = 1;
}

public enum ParameterModifier
{
    None,
    Ref,
    Out,
    In,
    Params,
}
```

---

## Statementy (PROP-031 — nové)

> Statement hierarchie pro reprezentaci těl metod a konstruktorů jako AST.
> Nahrazuje `MethodElement.Body: string?` typově bezpečným `BlockStatement?`.

```
Src/MetaForge.Core/Elements/Statements/
├── Statement.cs              ← abstract base
├── StatementKind.cs          ← enum (Block, Return, If, For, While, Assignment, Expression)
├── BlockStatement.cs         ← { stmt1; stmt2; }
├── ReturnStatement.cs        ← return X;
├── IfStatement.cs            ← if (cond) { } else { }
├── ForStatement.cs           ← for (init; cond; inc) { }
├── WhileStatement.cs         ← while (cond) { }
├── AssignmentStatement.cs    ← varName = value;
└── ExpressionStatement.cs    ← volání metody jako příkaz
```

### Statement (abstract base)

```csharp
public abstract class Statement
{
    public abstract StatementKind StatementKind { get; }
}

public enum StatementKind
{
    Block, Return, If, For, While, Assignment, Expression
}
```

### BlockStatement

```csharp
public sealed class BlockStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Block;
    public List<Statement> Statements { get; } = [];
    
    public BlockStatement() { }
    public BlockStatement(params Statement[] statements) => Statements.AddRange(statements);
}
```

### ReturnStatement

```csharp
public sealed class ReturnStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Return;
    public Expression? Value { get; init; }  // null = return; (void)
}
```

### IfStatement

```csharp
public sealed class IfStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.If;
    public Expression Condition { get; init; } = default!;
    public Statement? TrueBranch { get; init; }
    public Statement? FalseBranch { get; init; }  // null = chybí else
}
```

### ForStatement

```csharp
public sealed class ForStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.For;
    public string Variable { get; init; } = string.Empty;
    public Expression Start { get; init; } = default!;
    public Expression End { get; init; } = default!;
    public Statement? Body { get; init; }
}
```

### WhileStatement

```csharp
public sealed class WhileStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.While;
    public Expression Condition { get; init; } = default!;
    public Statement Body { get; init; } = default!;
}
```

### AssignmentStatement

```csharp
public sealed class AssignmentStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Assignment;
    public string Variable { get; init; } = string.Empty;
    public Expression Value { get; init; } = default!;
}
```

### ExpressionStatement

```csharp
public sealed class ExpressionStatement : Statement
{
    public override StatementKind StatementKind => StatementKind.Expression;
    public Expression Expr { get; init; } = default!;
}
```

---

## Factory metody (PROP-033 — nové)

> **Aktualizace:** PROP-033 (2026-07-05) — statické factory metody pro validní modifikátorové kombinace dle integrační matice.
> Každá factory metoda = právě jedna ✅ řádka z `Docs/Integration/01-Integration-Test-Matrix.md`.
> Žádné tiché přepisování — atomická konstrukce validního elementu.

### Princip

- **Factory metody** pro modifikátorové kombinace (Class C1-C8, Method M1-M7, atd.)
- **Fluent `With*` metody** pro bezpečné vlastnosti bez konfliktů (access modifier, base class)
- **Properties zůstávají `{ get; set; }`** — factory metody jsou doplněk, ne náhrada
- **`CoreValidator`** v `Src/MetaForge.Core/Validation/` pro explicitní ověření

### ClassElement — factory metody

```csharp
// Modifikátorové kombinace (C1-C8)
ClassElement.Basic("Foo")           // public class Foo
ClassElement.Abstract("Foo")        // public abstract class Foo
ClassElement.Sealed("Foo")          // public sealed class Foo
ClassElement.Static("Foo")          // public static class Foo
ClassElement.Partial("Foo")         // public partial class Foo
ClassElement.Record("Foo")          // public record class Foo
ClassElement.AbstractRecord("Foo")  // public abstract record class Foo
ClassElement.SealedRecord("Foo")    // public sealed record class Foo

// Fluent rozšíření
.WithAccess(AccessModifier.Internal)         // A1,A2,A6
.WithBaseClass("BaseEntity")                 // I1-I4
.WithInterfaces("IDisposable", "ICloneable") // I3,I4
.WithUsings("System", "System.Linq")
.WithProperty(propertyElement)
.WithMethod(methodElement)
```

### EnumElement — factory metody

```csharp
EnumElement.Basic("Status")                    // E1: int32
EnumElement.ByteEnum("Status")                 // E2: byte
EnumElement.Int64Enum("Status")                // E3: long
EnumElement.Flags("Permissions")                // E4: [Flags] int32
EnumElement.Flags("Permissions", DataType.Byte) // E4: varianta

.WithAccess(AccessModifier.Internal)
.WithMember(member)
.WithMembers(m1, m2, m3)
```

### StructElement — factory metody

```csharp
StructElement.Basic("Point")              // S1
StructElement.ReadOnly("Point")           // S2
StructElement.Record("Point")             // S3
StructElement.ReadOnlyRecord("Point")     // S4
```

### InterfaceElement — factory metody

```csharp
InterfaceElement.Basic("IRepository")
```

### PropertyElement — factory metody

```csharp
PropertyElement.GetSet("Name", TypeModel.String)          // P1
PropertyElement.GetOnly("Id", TypeModel.Guid)             // P2
PropertyElement.InitOnly("Name", TypeModel.String)        // P3
PropertyElement.Required("Name", TypeModel.String)        // P4
PropertyElement.Static("Instance", TypeModel.Object)      // P5
PropertyElement.RequiredGetOnly("Id", TypeModel.Guid)     // P8
```

### MethodElement — factory metody

```csharp
MethodElement.Basic("Execute")                            // M1: void
MethodElement.Static("Calc", TypeModel.Decimal)           // M2
MethodElement.Async("Fetch", TypeModel.Of(DataType.Task)) // M3/M8
MethodElement.Abstract("Get", TypeModel.String)           // M5
MethodElement.Virtual("OnEvent", TypeModel.Void)          // M6
MethodElement.Override("ToString", TypeModel.String)      // M7
```

### CoreValidator

```csharp
// Složka: Src/MetaForge.Core/Validation/
// Soubory: ValidationIssue.cs, CoreValidator.cs

// Vrací seznam problémů — prázdný = validní
var issues = CoreValidator.Validate(classElement);

// Fail-fast — vyhodí InvalidOperationException
CoreValidator.EnsureValid(classElement);

// Specializované validátory pro member elementy
var methodIssues = CoreValidator.ValidateMethod(methodElement);
var propIssues = CoreValidator.ValidateProperty(propertyElement);
```

Pokryté ❌ řádky matice: C9, C10, C12, A3-A5, I5, E5-E6, P7, T19-T21, M9-M12, B11-B13.

