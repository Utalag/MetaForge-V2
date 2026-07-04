# Generators

> CSharpGenerator, templates, package manifest, language mapping

---

## Princip

- Generátory čtou Core elementy (ClassElement, InterfaceElement, EnumElement).
- Jediný aktivní jazyk je C#; `LanguageMapping` existuje pro metadata/export.
- Výstupem je `GeneratedCodeArtifact`.

## CSharpGenerator

```csharp
public class CSharpGenerator : BaseCodeGenerator
{
    public override string LanguageId => "csharp";
    public override GeneratedCodeArtifact Generate(RootElement element) { }
}
```

## BaseCodeGenerator

```csharp
public abstract class BaseCodeGenerator
{
    public abstract string LanguageId { get; }
    public abstract GeneratedCodeArtifact Generate(RootElement element);
    protected string RenderTemplate(string templateName, object model) { }
}
```

## GeneratedCodeArtifact

```csharp
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    string LanguageId,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);
```

## TemplateManager

```csharp
public class TemplateManager
{
    public string Render(string templateName, object model) { }
    public void RegisterTemplate(string name, string templateContent) { }
}
```

## LanguageMapping (metadata only — C#-first, jeden jazyk)

```csharp
public sealed record LanguageMapping(
    string LanguageId,
    string FileExtension,
    string CommentPrefix,
    bool SupportsPartialClasses
);
```

## PackageManifestGenerator

```csharp
public class PackageManifestGenerator
{
    public string GenerateManifest(ForgeBlockPackage package) { }
}
```