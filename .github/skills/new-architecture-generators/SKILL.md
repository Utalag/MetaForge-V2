---
name: new-architecture-generators
description: "Pouzij pri: praci s Generators vrstvou — CSharpGenerator, BaseCodeGenerator, GeneratedCodeArtifact, LanguageMapping, DiagnosticInfo."
---

# new-architecture-generators

Řídit implementaci Generators vrstvy dle `10-Generators.md`. Hlídat C#-first princip — jediný aktivní generátor je CSharpGenerator.

## Kdy použít

- Při práci se soubory v `Src/MetaForge.Generators/`
- Při implementaci CSharpGenerator, BaseCodeGenerator
- Při generování C# kódu z Core elementů

## Principy

| # | Princip | Důsledek |
|---|---------|----------|
| 1 | **C#-first** | Jediný aktivní generátor je CSharpGenerator |
| 2 | **Generátory čtou Core elementy** | Vstupem jsou ClassElement, InterfaceElement, EnumElement atd. |
| 3 | **Výstupem je GeneratedCodeArtifact** | Jednotný výstupní typ pro všechny generátory |
| 4 | **BaseCodeGenerator je abstrakce** | Společná logika (LanguageId, FileExtension) |
| 5 | **LanguageMapping je metadata-only** | Nejedná se o aktivní generátor |

## Klíčové typy

### BaseCodeGenerator

```csharp
public abstract class BaseCodeGenerator
{
    public abstract string LanguageId { get; }
    public abstract string FileExtension { get; }
    public abstract GeneratedCodeArtifact Generate(RootElement element);
    public virtual IReadOnlyList<GeneratedCodeArtifact> GenerateAll(IEnumerable<RootElement> elements);
}
```

### CSharpGenerator

```csharp
public sealed class CSharpGenerator : BaseCodeGenerator
{
    public override string LanguageId => "csharp";
    public override string FileExtension => ".cs";
    public override GeneratedCodeArtifact Generate(RootElement element);
}
```

### GeneratedCodeArtifact

```csharp
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    string LanguageId,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);
```

### DiagnosticInfo

```csharp
public sealed record DiagnosticInfo(
    string Message,
    DiagnosticSeverity Severity = DiagnosticSeverity.Warning,
    string? ElementId = null,
    string? ElementName = null
);
```

## Workflow generování

```
RootElement → BaseCodeGenerator.Generate()
                → CSharpGenerator.Generate() (C#-first)
                    → string (C# source code)
                        → GeneratedCodeArtifact
```

## Mapování DataType → C# klíčová slova

| DataType | C# |
|----------|-----|
| Bool | bool |
| Int32 | int |
| Int64 | long |
| String | string |
| Decimal | decimal |
| Double | double |
| Single | float |
| Guid | Guid |
| DateTime | DateTime |
| DateOnly | DateOnly |
| Void | void |
| Object | object |

## Anti-patterny

- ❌ Přidávání generátorů pro jiné jazyky (C#-first)
- ❌ LanguageMapping používaný jako aktivní generátor
- ❌ Generátor závislý na BusinessModel (může číst jen Core elementy)
- ❌ Generovaný kód, který není kompilabilní

## Výstupní checklist

- [ ] Generovaný C# je kompilabilní
- [ ] BaseCodeGenerator abstrakce je dodržena
- [ ] LanguageMapping je metadata-only
- [ ] CSharpGenerator.LanguageId = "csharp"
- [ ] GeneratedCodeArtifact má konzistentní formát
