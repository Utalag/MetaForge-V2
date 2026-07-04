# Core — Behaviors

> Expression, Constraints, Boundary Analysis, StandardLibraries

---

## Expression

```csharp
public abstract class Expression
{
    public abstract string Kind { get; }
}
```

## ComputedExpression

```csharp
public class ComputedExpression : Expression
{
    public override string Kind => "Computed";
    public ComputedOperation Operation { get; set; }
    public List<Expression> Operands { get; } = new();
}
```

## IConstraintInferencer

```csharp
public interface IConstraintInferencer
{
    IReadOnlyList<string> Infer(string attributeName, TypeModel type);
}
```

## IStandardLibraryTranslator

```csharp
public interface IStandardLibraryTranslator
{
    string OperationId { get; }
    StandardLibraryRequirements? Translate(ComputedOperation operation);
}
```
