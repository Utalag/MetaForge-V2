# Epic 7 — Generators (C#-first)

> **Cíl:** Vytvořit projekt `MetaForge.Generators` s CSharpGenerator jako jediným aktivním generátorem.
> **Výstup:** Generátor, který převádí Core elementy na kompilovatelný C# kód.
> **Závislosti:** Epic 2 (Core elementy).

---

## DŮLEŽITÉ: C# je JEDINÝ výstupní jazyk

- `CSharpGenerator.LanguageId = "csharp"` — jediný aktivní generátor.
- `LanguageMapping` existuje pro metadata/export, ale není potřeba pro v1.

---

## TASK-7.1.1 — Založení projektu MetaForge.Generators

**Vstup:** `MetaForge.slnx`, Epic 2 dokončen.
**Výstup:** Class library projekt `Src/MetaForge.Generators/MetaForge.Generators.csproj`.
**Soubory:** `Src/MetaForge.Generators/MetaForge.Generators.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.Generators.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.Generators</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\MetaForge.Core\MetaForge.Core.csproj" />
  </ItemGroup>
</Project>
```

**Ověření:** `dotnet build Src/MetaForge.Generators/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt ze slnx, smaž složku.

---

## TASK-7.2.1 — GeneratedCodeArtifact + DiagnosticInfo + BaseCodeGenerator

**Vstup:** TASK-7.1.1 (projekt existuje).
**Výstup:** 3 soubory v kořeni `Generators/`.
**Soubory:**
- `Src/MetaForge.Generators/GeneratedCodeArtifact.cs`
- `Src/MetaForge.Generators/DiagnosticInfo.cs`
- `Src/MetaForge.Generators/BaseCodeGenerator.cs`

**Kód — `DiagnosticInfo.cs`:**

```csharp
namespace MetaForge.Generators;

/// <summary>
/// Diagnostická informace — varování nebo chyba při generování.
/// </summary>
public sealed record DiagnosticInfo(
    string Message,
    DiagnosticSeverity Severity = DiagnosticSeverity.Warning,
    string? ElementId = null,
    string? ElementName = null
);

public enum DiagnosticSeverity
{
    Warning,
    Error,
}
```

**Kód — `GeneratedCodeArtifact.cs`:**

```csharp
namespace MetaForge.Generators;

/// <summary>
/// Výstup generátoru — vygenerovaný soubor s kódem.
/// </summary>
public sealed record GeneratedCodeArtifact(
    string FileName,
    string SourceCode,
    string LanguageId,
    IReadOnlyList<DiagnosticInfo>? Diagnostics = null
);
```

**Kód — `BaseCodeGenerator.cs`:**

```csharp
using MetaForge.Core.Abstractions;

namespace MetaForge.Generators;

/// <summary>
/// Abstraktní bázová třída pro všechny generátory kódu.
/// </summary>
public abstract class BaseCodeGenerator
{
    /// <summary>Identifikátor jazyka (např. "csharp").</summary>
    public abstract string LanguageId { get; }

    /// <summary>Přípona souboru (např. ".cs").</summary>
    public abstract string FileExtension { get; }

    /// <summary>Vygeneruje kód pro daný RootElement.</summary>
    public abstract GeneratedCodeArtifact Generate(RootElement element);

    /// <summary>Vygeneruje kód pro více elementů najednou (např. celý namespace).</summary>
    public virtual IReadOnlyList<GeneratedCodeArtifact> GenerateAll(IEnumerable<RootElement> elements)
    {
        var results = new List<GeneratedCodeArtifact>();
        foreach (var element in elements)
        {
            var artifact = Generate(element);
            results.Add(artifact);
        }
        return results.AsReadOnly();
    }
}
```

**Ověření:** `dotnet build` projde.
**Riziko:** Nízké.
**Rollback:** Smaž všechny 3 soubory.

---

## TASK-7.3.1 — CSharpGenerator

**Vstup:** TASK-7.2.1 (BaseCodeGenerator, GeneratedCodeArtifact), Core elementy z Epic 2.
**Výstup:** Soubor `Src/MetaForge.Generators/CSharp/CSharpGenerator.cs`.
**Soubory:** `Src/MetaForge.Generators/CSharp/CSharpGenerator.cs`

**Kód:**

```csharp
using System.Text;
using MetaForge.Core.Abstractions;
using MetaForge.Core.Elements.Members;
using MetaForge.Core.Elements.Types;

namespace MetaForge.Generators.CSharp;

/// <summary>
/// Generátor C# kódu — jediný aktivní generátor.
/// Převádí Core elementy na kompilovatelný C# kód.
/// </summary>
public sealed class CSharpGenerator : BaseCodeGenerator
{
    public override string LanguageId => "csharp";
    public override string FileExtension => ".cs";

    public override GeneratedCodeArtifact Generate(RootElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Name))
        {
            return new GeneratedCodeArtifact(
                FileName: "error.cs",
                SourceCode: "// ERROR: Element bez názvu",
                LanguageId: LanguageId,
                Diagnostics: new[] { new DiagnosticInfo("Element nemá nastavený název.", DiagnosticSeverity.Error, element.Id.ToString()) }
            );
        }

        var code = element switch
        {
            ClassElement cls => GenerateClass(cls),
            InterfaceElement iface => GenerateInterface(iface),
            EnumElement enm => GenerateEnum(enm),
            StructElement str => GenerateStruct(str),
            _ => $"// Nepodporovaný element typu: {element.GetType().Name}"
        };

        var diagnostics = new List<DiagnosticInfo>();
        if (string.IsNullOrWhiteSpace(code) || code.StartsWith("// Nepodporovaný"))
        {
            diagnostics.Add(new DiagnosticInfo($"Nepodporovaný element: {element.Kind}", DiagnosticSeverity.Warning, element.Id.ToString(), element.Name));
        }

        return new GeneratedCodeArtifact(
            FileName: $"{element.Name}{FileExtension}",
            SourceCode: code,
            LanguageId: LanguageId,
            Diagnostics: diagnostics.Count > 0 ? diagnostics.AsReadOnly() : null
        );
    }

    // === Generování jednotlivých typů ===

    private static string GenerateClass(ClassElement cls)
    {
        var sb = new StringBuilder();

        // Usingy
        foreach (var u in cls.Usings)
            sb.AppendLine($"using {u};");
        if (cls.Usings.Count > 0) sb.AppendLine();

        // Atributy
        foreach (var attr in cls.Attributes)
            sb.AppendLine(GenerateAttribute(attr));

        // Modifikátory
        var modifiers = BuildModifiers(
            cls.AccessModifier,
            isAbstract: cls.IsAbstract,
            isSealed: cls.IsSealed,
            isStatic: cls.IsStatic,
            isPartial: cls.IsPartial
        );

        // Deklarace třídy
        sb.Append($"{modifiers}class {cls.Name}");

        // Dědičnost
        var inheritance = new List<string>();
        if (!string.IsNullOrWhiteSpace(cls.BaseClassName))
            inheritance.Add(cls.BaseClassName);
        inheritance.AddRange(cls.ImplementedInterfaces);

        if (inheritance.Count > 0)
            sb.Append($" : {string.Join(", ", inheritance)}");

        sb.AppendLine();
        sb.AppendLine("{");

        // Properties
        foreach (var prop in cls.Properties)
        {
            sb.AppendLine(GenerateProperty(prop, "    "));
        }

        if (cls.Properties.Count > 0 && cls.Methods.Count > 0)
            sb.AppendLine();

        // Metody
        foreach (var method in cls.Methods)
        {
            sb.AppendLine(GenerateMethod(method, "    "));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateInterface(InterfaceElement iface)
    {
        var sb = new StringBuilder();

        var modifiers = BuildModifiers(iface.AccessModifier);
        sb.AppendLine($"{modifiers}interface {iface.Name}");
        sb.AppendLine("{");

        foreach (var prop in iface.Properties)
        {
            var getter = prop.HasGetter ? "get; " : "";
            var setter = prop.HasSetter ? "set; " : "";
            var init = prop.IsInitOnly ? "init; " : "";
            sb.AppendLine($"    {MapType(prop.Type)} {prop.Name} {{ {getter}{setter}{init}}}");
        }

        foreach (var method in iface.Methods)
        {
            sb.AppendLine(GenerateMethodSignature(method, "    ") + ";");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateEnum(EnumElement enm)
    {
        var sb = new StringBuilder();

        var modifiers = BuildModifiers(enm.AccessModifier);
        var flagsAttr = enm.IsFlags ? $"[Flags]{Environment.NewLine}" : "";
        var underlying = enm.UnderlyingType != MetaForge.Core.DataTypes.DataType.Int32
            ? $" : {MapDataTypeToCSharp(enm.UnderlyingType)}"
            : "";

        sb.Append(flagsAttr);
        sb.AppendLine($"{modifiers}enum {enm.Name}{underlying}");
        sb.AppendLine("{");

        for (int i = 0; i < enm.Members.Count; i++)
        {
            var member = enm.Members[i];
            var comma = i < enm.Members.Count - 1 ? "," : "";
            var value = member.Value is not null ? $" = {member.Value}" : "";
            sb.AppendLine($"    {member.Name}{value}{comma}");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateStruct(StructElement str)
    {
        var sb = new StringBuilder();

        var modifiers = BuildModifiers(str.AccessModifier);
        var readOnly = str.IsReadOnly ? "readonly " : "";
        var record = str.IsRecord ? "record " : "";

        sb.AppendLine($"{modifiers}{readOnly}{record}struct {str.Name}");
        sb.AppendLine("{");

        foreach (var prop in str.Properties)
            sb.AppendLine(GenerateProperty(prop, "    "));

        foreach (var method in str.Methods)
            sb.AppendLine(GenerateMethod(method, "    "));

        sb.AppendLine("}");

        return sb.ToString();
    }

    // === Pomocné metody ===

    private static string GenerateProperty(PropertyElement prop, string indent)
    {
        var sb = new StringBuilder();

        var accessMod = prop.AccessModifier != AccessModifier.Public
            ? $"{MapAccessModifier(prop.AccessModifier)} " : "";
        var staticMod = prop.IsStatic ? "static " : "";
        var requiredMod = prop.IsRequired ? "required " : "";

        var typeName = MapType(prop.Type);

        sb.Append($"{indent}{accessMod}{staticMod}{requiredMod}{typeName} {prop.Name}");

        // Getter/Setter
        var getter = prop.HasGetter ? "get; " : "";
        var setter = prop.HasSetter ? (prop.IsInitOnly ? "init; " : "set; ") : "";
        sb.Append($" {{ {getter}{setter}}}");

        // Výchozí hodnota
        if (!string.IsNullOrWhiteSpace(prop.DefaultValue))
            sb.Append($" = {prop.DefaultValue};");
        else
            sb.Append(';');

        return sb.ToString();
    }

    private static string GenerateMethod(MethodElement method, string indent)
    {
        var sb = new StringBuilder();
        sb.Append(GenerateMethodSignature(method, indent));

        if (!string.IsNullOrWhiteSpace(method.Body))
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    {method.Body}");
            sb.AppendLine($"{indent}}}");
        }
        else
        {
            // Abstraktní nebo interface metoda
            if (method.IsAbstract)
                sb.Append(';');
            else
            {
                sb.AppendLine();
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    throw new NotImplementedException();");
                sb.AppendLine($"{indent}}}");
            }
        }

        return sb.ToString();
    }

    private static string GenerateMethodSignature(MethodElement method, string indent)
    {
        var accessMod = method.AccessModifier != AccessModifier.Public
            ? $"{MapAccessModifier(method.AccessModifier)} " : "";
        var staticMod = method.IsStatic ? "static " : "";
        var asyncMod = method.IsAsync ? "async " : "";
        var abstractMod = method.IsAbstract ? "abstract " : "";
        var virtualMod = method.IsVirtual ? "virtual " : "";
        var overrideMod = method.IsOverride ? "override " : "";

        var returnType = MapType(method.ReturnType);
        if (method.IsAsync && method.ReturnType.BaseType == MetaForge.Core.DataTypes.DataType.Void)
            returnType = "Task";
        else if (method.IsAsync && returnType != "void")
            returnType = $"Task<{returnType}>";

        var parameters = string.Join(", ", method.Parameters.Select(MapParameter));

        return $"{indent}{accessMod}{staticMod}{asyncMod}{abstractMod}{virtualMod}{overrideMod}{returnType} {method.Name}({parameters})";
    }

    private static string MapParameter(ParameterElement p)
    {
        var modifier = p.Modifier switch
        {
            ParameterModifier.Ref => "ref ",
            ParameterModifier.Out => "out ",
            ParameterModifier.In => "in ",
            ParameterModifier.Params => "params ",
            _ => "",
        };

        var type = MapType(p.Type);
        var defaultValue = p.HasDefaultValue && p.DefaultValue is not null
            ? $" = {p.DefaultValue}"
            : "";

        return $"{modifier}{type} {p.Name}{defaultValue}";
    }

    private static string GenerateAttribute(AttributeElement attr)
    {
        var args = attr.Arguments.Count > 0
            ? string.Join(", ", attr.Arguments.Select(a => a is string s ? $"\"{s}\"" : a?.ToString() ?? "null"))
            : "";
        return $"[{attr.Name}{(args.Length > 0 ? $"({args})" : "")}]";
    }

    // === Mapování typů ===

    private static string MapType(MetaForge.Core.DataTypes.TypeModel type)
    {
        var nullable = type.IsNullable ? "?" : "";
        var baseType = MapDataTypeToCSharp(type.BaseType);

        // Pokud má custom název, použij ho
        if (!string.IsNullOrWhiteSpace(type.CustomTypeName))
            return $"{type.CustomTypeName}{nullable}";

        // Kolekce
        if (type.IsCollection)
        {
            var innerType = type.GenericArguments.Count > 0
                ? MapType(type.GenericArguments[0])
                : "object";
            return $"List<{innerType}>";
        }

        return $"{baseType}{nullable}";
    }

    private static string MapDataTypeToCSharp(MetaForge.Core.DataTypes.DataType dataType) => dataType switch
    {
        MetaForge.Core.DataTypes.DataType.Bool => "bool",
        MetaForge.Core.DataTypes.DataType.Byte => "byte",
        MetaForge.Core.DataTypes.DataType.SByte => "sbyte",
        MetaForge.Core.DataTypes.DataType.Int16 => "short",
        MetaForge.Core.DataTypes.DataType.UInt16 => "ushort",
        MetaForge.Core.DataTypes.DataType.Int32 => "int",
        MetaForge.Core.DataTypes.DataType.UInt32 => "uint",
        MetaForge.Core.DataTypes.DataType.Int64 => "long",
        MetaForge.Core.DataTypes.DataType.UInt64 => "ulong",
        MetaForge.Core.DataTypes.DataType.Int128 => "Int128",
        MetaForge.Core.DataTypes.DataType.Half => "Half",
        MetaForge.Core.DataTypes.DataType.Single => "float",
        MetaForge.Core.DataTypes.DataType.Double => "double",
        MetaForge.Core.DataTypes.DataType.Decimal => "decimal",
        MetaForge.Core.DataTypes.DataType.NInt => "nint",
        MetaForge.Core.DataTypes.DataType.NUInt => "nuint",
        MetaForge.Core.DataTypes.DataType.Char => "char",
        MetaForge.Core.DataTypes.DataType.String => "string",
        MetaForge.Core.DataTypes.DataType.Binary => "byte[]",
        MetaForge.Core.DataTypes.DataType.DateOnly => "DateOnly",
        MetaForge.Core.DataTypes.DataType.TimeOnly => "TimeOnly",
        MetaForge.Core.DataTypes.DataType.DateTime => "DateTime",
        MetaForge.Core.DataTypes.DataType.DateTimeOffset => "DateTimeOffset",
        MetaForge.Core.DataTypes.DataType.TimeSpan => "TimeSpan",
        MetaForge.Core.DataTypes.DataType.Guid => "Guid",
        MetaForge.Core.DataTypes.DataType.Uri => "Uri",
        MetaForge.Core.DataTypes.DataType.Version => "Version",
        MetaForge.Core.DataTypes.DataType.Object => "object",
        MetaForge.Core.DataTypes.DataType.Dynamic => "dynamic",
        MetaForge.Core.DataTypes.DataType.Void => "void",
        _ => "object",
    };

    private static string BuildModifiers(AccessModifier access, bool isAbstract = false, bool isSealed = false, bool isStatic = false, bool isPartial = false)
    {
        var parts = new List<string>();
        parts.Add(MapAccessModifier(access));
        if (isStatic) parts.Add("static");
        if (isAbstract) parts.Add("abstract");
        if (isSealed) parts.Add("sealed");
        if (isPartial) parts.Add("partial");
        return string.Join(" ", parts) + " ";
    }

    private static string MapAccessModifier(AccessModifier access) => access switch
    {
        AccessModifier.Public => "public",
        AccessModifier.Internal => "internal",
        AccessModifier.Protected => "protected",
        AccessModifier.Private => "private",
        AccessModifier.ProtectedInternal => "protected internal",
        AccessModifier.PrivateProtected => "private protected",
        _ => "public",
    };
}
```

**Ověření:** `dotnet build` projde.
- Vytvoř `ClassElement { Name = "Customer" }` s Property `Name : string` → CSharpGenerator.Generate vrátí validní C# kód.
- Vygenerovaný kód obsahuje `public class Customer`.
**Riziko:** Střední — generátor musí produkovat kompilovatelný C# kód.
**Rollback:** Smaž soubor.

---

## TASK-7.3.2 — LanguageMapping (metadata)

**Vstup:** TASK-7.1.1.
**Výstup:** Soubor `Src/MetaForge.Generators/LanguageMapping.cs`.
**Soubory:** `Src/MetaForge.Generators/LanguageMapping.cs`

**Kód:**

```csharp
namespace MetaForge.Generators;

/// <summary>
/// Metadata o cílovém jazyce — pro C#-first architekturu existuje jen jedna instance.
/// </summary>
public sealed record LanguageMapping(
    string LanguageId,
    string FileExtension,
    string CommentPrefix,
    bool SupportsPartialClasses
)
{
    /// <summary>Výchozí mapping pro C#.</summary>
    public static LanguageMapping CSharp { get; } = new(
        LanguageId: "csharp",
        FileExtension: ".cs",
        CommentPrefix: "//",
        SupportsPartialClasses: true
    );
}
```

**Ověření:** `dotnet build` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## Souhrn Epic 7 — Co musí existovat po dokončení

```
Src/MetaForge.Generators/
├── MetaForge.Generators.csproj
├── GeneratedCodeArtifact.cs
├── DiagnosticInfo.cs
├── BaseCodeGenerator.cs
├── LanguageMapping.cs
└── CSharp/
    └── CSharpGenerator.cs
```

**Celkem souborů:** ~6
**Build:** `dotnet build Src/MetaForge.Generators/` projde.

**Checkpoint:** `git tag checkpoint/epic-7-done`
