# Core Support Matrix — Přehled podpory C# konstrukcí

> Indexová matice všech C# konstrukcí a stav jejich podpory v Core modelu.
> Slouží jako živý backlog — každý `Planned` nebo `Partial` řádek je kandidát na PROP.

## Legenda stavů

| Stav | Význam |
|------|--------|
| ✅ Supported | Plně podporováno — roundtrip beze ztráty |
| 🔵 Partial | Částečně podporováno — některé aspekty chybí |
| 🟡 Planned | Plánováno — existuje PROP nebo backlog položka |
| ❌ Unsupported | Není podporováno — mimo scope Core |

## Typové druhy (Type Kinds)

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Class | ✅ Supported | `ClassElement` (Kind="class") | PROP-002 | — |
| Class with auto-properties | ✅ Supported | `ClassElement` + `PropertyElement` | PROP-002 | — |
| Sealed Class | ✅ Supported | `ClassElement.IsSealed` | PROP-024 | — |
| Abstract Class | ✅ Supported | `ClassElement.IsAbstract` | PROP-024 | — |
| Static Class | ✅ Supported | `ClassElement.IsStatic` | PROP-024 | — |
| Partial Class | 🔵 Partial | `ClassElement.IsPartial` (bez merge sémantiky) | PROP-025 | Medium |
| Record Class | 🔵 Partial | `ClassElement.IsRecord` | PROP-024 | High |
| Record Struct | 🟡 Planned | `StructElement.IsRecord` | — | Medium |
| Struct | ✅ Supported | `StructElement` (Kind="struct") | PROP-002 | — |
| ReadOnly Struct | ✅ Supported | `StructElement.IsReadOnly` | PROP-024 | — |
| Interface | ✅ Supported | `InterfaceElement` (Kind="interface") | PROP-002 | — |
| Generic Interface | 🔵 Partial | `InterfaceElement.TypeParameters` (bez variance) | PROP-035 | Medium |
| Enum | ✅ Supported | `EnumElement` (Kind="enum") | PROP-002 | — |
| Flags Enum | 🔵 Partial | `EnumElement.IsFlags` | — | Low |
| Delegate | 🟡 Planned | `DelegateElement` | PROP-037 | Medium |
| Event | 🟡 Planned | `EventElement` | PROP-037 | Medium |
| Operator | 🟡 Planned | `OperatorElement` | PROP-037 | Medium |

## Členy typů (Members)

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Method (signatura) | ✅ Supported | `MethodElement` | PROP-002 | — |
| Method (tělo jako string) | ✅ Supported | `MethodElement.Body` = string (generátor) | PROP-002 | — |
| Method (tělo jako AST) | 🔵 Partial | `MethodElement.Body` = `BlockStatement` | PROP-031 | 🔵 |
| Method (expression body) | ✅ Supported | `MethodElement.ExpressionBody` | PROP-035 | — |
| Async Method | ✅ Supported | `MethodElement.IsAsync` | PROP-024 | — |
| Abstract Method | ✅ Supported | `MethodElement.IsAbstract` | PROP-024 | — |
| Virtual Method | ✅ Supported | `MethodElement.IsVirtual` | PROP-024 | — |
| Override Method | ✅ Supported | `MethodElement.IsOverride` | PROP-024 | — |
| Extension Method | ✅ Supported | `MethodElement.IsExtension` | PROP-035 | — |
| Generic Method | ✅ Supported | `MethodElement.TypeParameters` + `TypeConstraints` | PROP-035 | — |
| Property (get/set) | ✅ Supported | `PropertyElement` (HasGetter/HasSetter) | PROP-002 | — |
| Property (get-only) | ✅ Supported | `PropertyElement.HasGetter && !HasSetter` | PROP-024 | — |
| Property (init-only) | 🔵 Partial | `PropertyElement.IsInitOnly` | — | Medium |
| Property (required) | 🔵 Partial | `PropertyElement.IsRequired` | PROP-024 | High |
| Static Property | ✅ Supported | `PropertyElement.IsStatic` | PROP-024 | — |
| Field | 🔵 Partial | `FieldElement` (existuje, omezené použití) | — | Low |
| Constructor | 🔵 Partial | `ConstructorElement` (existuje) | — | Medium |
| Primary Constructor | ❌ Unsupported | — | — | Low |

## Výrazy (Expressions)

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Constant (literal) | ✅ Supported | `ConstantExpression` | PROP-024 | — |
| Member Access | ✅ Supported | `MemberAccessExpression` | PROP-024 | — |
| Binary Operation | ✅ Supported | `BinaryExpression` + `BinaryOperator` (15 ops) | PROP-024 | — |
| Unary Operation | ✅ Supported | `UnaryExpression` + `UnaryOperator` (5 ops) | PROP-024 | — |
| Method Call | ✅ Supported | `MethodCallExpression` + `NamedArgument` | PROP-035 | — |
| Lambda | ✅ Supported | `LambdaExpression` | PROP-035 | — |
| Object Creation (new) | ✅ Supported | `NewExpression` + `MemberBinding` | PROP-035 | — |
| Conditional (ternary) | ✅ Supported | `ConditionalExpression` | PROP-024 | — |
| Default | ✅ Supported | `DefaultExpression` | PROP-035 | — |
| Conversion (cast) | ✅ Supported | `ConversionExpression` | PROP-035 | — |
| Await | ✅ Supported | `AwaitExpression` | PROP-035 | — |
| Switch Expression | ✅ Supported | `SwitchExpression` + `SwitchArm` | PROP-035 | — |
| Is Pattern | ✅ Supported | `IsPatternExpression` + `PatternKind` | PROP-035 | — |
| Null Coalescing | ✅ Supported | `NullCoalescingExpression` | PROP-035 | — |

## Statements (AST)

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Block | ✅ Supported | `BlockStatement` | PROP-031 | — |
| Return | ✅ Supported | `ReturnStatement` | PROP-031 | — |
| If/Else | ✅ Supported | `IfStatement` | PROP-031 | — |
| For | ✅ Supported | `ForStatement` | PROP-031 | — |
| While | ✅ Supported | `WhileStatement` | PROP-031 | — |
| Assignment | ✅ Supported | `AssignmentStatement` | PROP-031 | — |
| Expression Statement | ✅ Supported | `ExpressionStatement` | PROP-031 | — |
| Switch | ✅ Supported | `SwitchStatement` + `SwitchCase` | PROP-031 | — |
| ForEach | ✅ Supported | `ForEachStatement` | PROP-031 | — |
| TryCatch | ✅ Supported | `TryCatchStatement` + `CatchClause` | PROP-031 | — |
| Using (block) | ✅ Supported | `UsingStatement` | PROP-031 | — |
| Using Declaration | ✅ Supported | `UsingDeclarationStatement` | PROP-031 | — |
| Local Function | ✅ Supported | `LocalFunctionStatement` | PROP-031 | — |

## Typový systém

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Primitivní typy | ✅ Supported | `DataType` (25+ hodnot) | PROP-002 | — |
| Nullable typy | ✅ Supported | `TypeModel.IsNullable` | PROP-002 | — |
| Kolekce | ✅ Supported | `TypeModel.IsCollection` | PROP-002 | — |
| Generic typy | ✅ Supported | `TypeModel.GenericArguments` | PROP-035 | — |
| Custom typy | ✅ Supported | `TypeModel.CustomTypeName` | PROP-002 | — |
| StrongType | ✅ Supported | `StrongType` record | PROP-024 | — |
| ValueObject validace | ✅ Supported | `ValueObjectValidationRule` | PROP-024 | — |
| Generic Constraints | ✅ Supported | `GenericConstraint` + `ConstraintKind` (7) | PROP-035 | — |

## Metadatový systém

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Namespace | ✅ Supported | `RootElement.Namespace` | PROP-035 | — |
| Using directives | ✅ Supported | `RootElement.Usings` | PROP-035 | — |
| XML Documentation | ✅ Supported | `RootElement.XmlSummary` | PROP-035 | — |
| Attributes | ✅ Supported | `AttributeElement` | PROP-002 | — |
| Metadata Bag | ✅ Supported | `MetadataBag` | PROP-038 | — |
| Access Modifiers | ✅ Supported | `AccessModifier` (6 hodnot) | PROP-002 | — |

## DX a Tooling

| Konstrukce | Stav | Core reprezentace | PROP | Priorita |
|-----------|------|-------------------|------|----------|
| Fluent Builders | ✅ Supported | 8 builder tříd | PROP-038 | — |
| DiagnosticBag | ✅ Supported | `DiagnosticBag` + 3 reportéry | PROP-038 | — |
| TransformPipeline | ✅ Supported | `TransformPipeline` + `IModelTransform` | PROP-038 | — |
| CoreValidator | ✅ Supported | `CoreValidator` + `ValidationIssue` | PROP-033 | — |
| Invarianty | ✅ Supported | `InvariantDefinition` + evaluator | PROP-036 | — |
| Snapshot Testy | ✅ Supported | `SnapshotComparer` | PROP-032 | — |

---

## Souhrn

| Kategorie | ✅ Supported | 🔵 Partial | 🟡 Planned | ❌ Unsupported |
|-----------|:---------:|:--------:|:--------:|:------------:|
| Type Kinds | 7 | 4 | 4 | 1 |
| Members | 11 | 5 | 1 | 1 |
| Expressions | 14 | 0 | 0 | 0 |
| Statements | 13 | 0 | 0 | 0 |
| Type System | 8 | 0 | 0 | 0 |
| Metadata | 6 | 0 | 0 | 0 |
| DX/Tooling | 6 | 0 | 0 | 0 |
| **Celkem** | **65** | **9** | **5** | **2** |
