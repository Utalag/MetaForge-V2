# Generators

> CodeGenerator, Scriban templates, TemplateManager, ExpressionRenderer, Packaging

---

## Princip

- **C#-only** — jediný výstupní jazyk, žádná polyglot abstrakce.
- Generátor čte Core elementy (`ClassElement`, `InterfaceElement`, `EnumElement`, `StructElement`).
- Generování probíhá přes **Scriban šablony** (`.scriban` soubory) — oddělení logiky od prezentace.
- Výstupem je `GeneratedCodeArtifact`.

---

## Adresářová struktura

```
Src/MetaForge.Generators/
├── MetaForge.Generators.csproj          (+ Scriban NuGet)
├── BaseCodeGenerator.cs                 (abstraktní báze)
├── TemplateManager.cs                   (Scriban loader + cache)
├── CodeGenerator.cs                     (jediný generátor)
├── ExpressionRenderer.cs                (ComputedExpression → C#)
├── GeneratedCodeArtifact.cs
├── DiagnosticInfo.cs
├── Templates/
│   ├── Class.scriban
│   ├── Interface.scriban
│   ├── Enum.scriban
│   ├── Struct.scriban
│   ├── Property.scriban
│   ├── Method.scriban
│   ├── Constructor.scriban
│   └── Field.scriban
└── Packaging/
    ├── IPackageManifestGenerator.cs
    ├── PackageManifestGenerator.cs
    ├── PackageManifestRegistry.cs
    └── GeneratedArtifactComposer.cs
```

---

## BaseCodeGenerator

```csharp
public abstract class BaseCodeGenerator
{
    protected TemplateManager Templates { get; } = TemplateManager.Instance;

    public abstract GeneratedCodeArtifact Generate(RootElement element);

    protected string RenderTemplate(string templateName, Dictionary<string, object?> model)
        => Templates.Render(templateName, model);
}
```

---

## CodeGenerator (jediný generátor)

```csharp
public sealed class CodeGenerator : BaseCodeGenerator
{
    private const string FileExtension = ".cs";

    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        // element switch → build Dictionary model → RenderTemplate("Class", model)
        // ClassElement     → Class.scriban
        // InterfaceElement → Interface.scriban
        // EnumElement      → Enum.scriban
        // StructElement    → Struct.scriban
    }
}
```

---

## TemplateManager

```csharp
public class TemplateManager
{
    // Loaduje .scriban soubory z Templates/, cachuje v ConcurrentDictionary
    // Render(templateName, model) → string

    public Template LoadTemplate(string templateName);
    public string Render(string templateName, object model);
    public string Render(string templateName, IDictionary<string, object?> model);
    public void ClearCache();

    public static TemplateManager Instance { get; }
}
```

NuGet: `Scriban 7.x` — výkonný template engine, liquid-syntax, bez runtime závislostí.

---

## Scriban šablony

### Class.scriban
```
{{ for using in usings }}using {{ using }};{{ end }}
namespace {{ namespace }};
{{ access_modifier }}{{ if is_static }} static{{ end }}... class {{ name }}{{ if base_class }} : {{ base_class }}{{ end }}
{
{{ for property in properties }}    {{ property }}{{ end }}
{{ for method in methods }}    {{ method }}{{ end }}
}
```

### Method.scriban
```
{{ access_modifier }}{{ if is_static }} static{{ end }}... {{ return_type }} {{ name }}({{ for param in parameters }}...)
{
{{ if body }}    {{ body }}{{ end }}
}
```

### Property.scriban
```
{{ access_modifier }}{{ if is_static }} static{{ end }} {{ type }} {{ name }} { {{ if has_getter }}get;{{ end }}{{ if has_setter }} set;{{ end }} }
```

### Enum.scriban, Interface.scriban, Struct.scriban
Obdobně — každý typ elementu má vlastní šablonu.

---

## GeneratedCodeArtifact

```csharp
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);
```

Bez `LanguageId` — výstup je vždy C#.

---

## DiagnosticInfo

```csharp
public sealed record DiagnosticInfo(
    string Message,
    DiagnosticSeverity Severity = DiagnosticSeverity.Warning,
    string? ElementId = null,
    string? ElementName = null
);

public enum DiagnosticSeverity { Warning, Error }
```

---

## ExpressionRenderer

```csharp
public sealed class ExpressionRenderer
{
    // Renderuje ComputedExpression → C# kód s podporou odsazení
    public string Render(ComputedExpression expr);
    public string Render(ComputedExpression expr, int indent);
}
```

### Podporované operace

| OperationId | Výstup | Příklad |
|-------------|--------|---------|
| `return` | `return expr;` | `return result;` |
| `assign` | `target = value;` | `count = 0;` |
| `declare-variable` | `type name = init;` | `int total = 0;` |
| `throw-if-null` | `if (x is null) throw ...` | Guard clause |
| `throw-if-empty` | `if (string.IsNullOrWhiteSpace(x)) throw ...` | String guard |
| `comparison` | `left op right` | `a == b` |
| `member-access` | `target.member` | `person.Name` |
| `string-format` | `$"..."` | Interpolated string |
| `binary` | `(left op right)` | `(a + b)` |
| `unary` | `op operand` | `!flag` |
| `ternary` | `cond ? true : false` | `a > 0 ? "pos" : "neg"` |
| `method-call` | `target.Method(args)` | `list.Add(item)` |
| `literal` | Hodnota | `42`, `"text"` |
| `variable-ref` | Název | `customerName` |
| `raw` | Surový kód | `=> expr` |
| `block` | `{ ... }` | Složený blok |
| `if` | `if (cond) then else` | Podmínka |
| `for` | `for (init; cond; incr) body` | Cyklus |
| `while` | `while (cond) body` | Cyklus |
| `throw` | `throw new Ex();` | Výjimka |

### Příklad stromové struktury

```
ComputedExpression (operation: "if")
 ├── Operand[0]: ComputedExpression (operation: "comparison")   → condition
 │    └── Operands: [age, >, 18]
 ├── Operand[1]: ComputedExpression (operation: "block")         → then
 │    └── Operands: [return "adult";]
 └── Operand[2]: ComputedExpression (operation: "block")         → else
      └── Operands: [return "minor";]
```

Výstup:
```csharp
if (age > 18)
{
    return "adult";
}
else
{
    return "minor";
}
```

---

## Packaging

```csharp
public interface IPackageManifestGenerator
{
    IReadOnlyCollection<GeneratedArtifactFile> GenerateFiles(
        IReadOnlyCollection<CodePackageDependency> packages);
}

public sealed class PackageManifestGenerator : IPackageManifestGenerator
{
    // Generuje NuGet .props soubor s PackageReference z CodePackageDependency
}

public static class PackageManifestRegistry
{
    public static void Register(IPackageManifestGenerator generator);
    public static IReadOnlyCollection<GeneratedArtifactFile> GenerateFiles(...);
}

public static class GeneratedArtifactComposer
{
    // Sloučí dílčí artifacty do jednoho výstupu + doplní package manifest
    public static GeneratedCodeArtifact Compose(IEnumerable<GeneratedCodeArtifact> artifacts);
}
```

---

## Workflow generování

```
RootElement → CodeGenerator.Generate()
                ├── ClassElement → build model (Dictionary) → RenderTemplate("Class", model)
                │                                                    └── Class.scriban
                ├── InterfaceElement → build model → RenderTemplate("Interface", model)
                │                                                    └── Interface.scriban
                ├── EnumElement → build model → RenderTemplate("Enum", model)
                │                                                    └── Enum.scriban
                └── StructElement → build model → RenderTemplate("Struct", model)
                                                         └── Struct.scriban
                                          ↓
                                   GeneratedCodeArtifact
```

---

## Co NENÍ v této vrstvě

| Není | Důvod |
|------|-------|
| `LanguageMapping` | C#-only — odstraněno |
| `LanguageId` na artifactu | C#-only — odstraněno |
| Polyglot expression renderery | C#-only |
| Polyglot ForgeBlock modely | C#-only |
| `plugin.json` | Metadata jsou v kódu, ne v JSONu |