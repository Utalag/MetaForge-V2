# Generators

> CodeGenerator, Scriban templates, TemplateManager, ExpressionRenderer (Expression & Statement AST), Packaging

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
├── ExpressionRenderer.cs                (Expression & Statement AST → C#)
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
    // Renderuje Expression a Statement AST → C# kód
    public string Render(BlockStatement block);           // tělo metody
    public string Render(BlockStatement block, int indent);
    public string RenderStatement(Statement stmt);        // dispatch (switch podle StatementKind)
    public string RenderExpression(Expression expr);      // dispatch (switch podle ExpressionKind)
}
```

### Podporované Statementy (výstup)

| Statement | Výstup | Příklad |
|-----------|--------|---------|
| ReturnStatement | `return expr;` | `return result;` |
| BlockStatement | `{ stmt1; stmt2; }` | Složený blok |
| IfStatement | `if (cond) { } else { }` | Podmínka |
| ForStatement | `for (int i = 0; i < n; i++) { }` | Cyklus |
| WhileStatement | `while (cond) { }` | Cyklus |
| AssignmentStatement | `var = value;` | `total = price * qty;` |
| ExpressionStatement | `expr;` | `list.Add(item);` |

### Podporované Expressiony (výstup)

| Expression | Výstup | Příklad |
|-----------|--------|---------|
| ConstantExpression | Hodnota | `42`, `"text"`, `true` |
| BinaryExpression | `(left op right)` | `(a + b)` |
| UnaryExpression | `op operand` | `!flag` |
| MethodCallExpression | `method(args)` | `Math.Sqrt(x)` |
| MemberAccessExpression | `target.member` | `person.Name` |
| ConditionalExpression | `cond ? true : false` | `a > 0 ? "yes" : "no"` |

### Příklad: Pythagorova věta jako AST

```csharp
var method = new MethodElement
{
    Name = "CalculateHypotenuse",
    ReturnType = TypeModel.Double,
    Body = new BlockStatement(
        new ReturnStatement(
            new MethodCallExpression("Math.Sqrt",
                new Expression[]
                {
                    new BinaryExpression(
                        new BinaryExpression(
                            new MemberAccessExpression("a"),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("a")),
                        BinaryOperator.Add,
                        new BinaryExpression(
                            new MemberAccessExpression("b"),
                            BinaryOperator.Multiply,
                            new MemberAccessExpression("b")))
                },
                TypeModel.Double)))
};
```

Výstup (přes `_renderer.Render(method.Body)`):
```csharp
{
    return Math.Sqrt((a * a) + (b * b));
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