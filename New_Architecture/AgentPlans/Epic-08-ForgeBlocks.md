# Epic 8 — ForgeBlock balíky

> **Cíl:** Vytvořit první ForgeBlock capability balíky — Math, String, Validation.
> **Výstup:** Tři ForgeBlock projekty registrované do ForgeBlockRegistry.
> **Závislosti:** Epic 2 (Core — ForgeBlockRegistry, IForgeBlockPackage).

---

## TASK-8.1.1 — Vytvoření složky ForgeBlocks a projektu ForgeBlock.Math

**Vstup:** `MetaForge.slnx`, Epic 2 dokončen (ForgeBlockRegistry).
**Výstup:** Projekt `Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj`.
**Soubory:** `Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.ForgeBlocks.Math.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.ForgeBlocks.Math</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\MetaForge.Core\MetaForge.Core.csproj" />
  </ItemGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej do složky `/Src/ForgeBlocks/`:

```xml
  <Folder Name="/Src/ForgeBlocks/">
    <Project Path="Src/ForgeBlocks/Math/MetaForge.ForgeBlocks.Math.csproj" />
  </Folder>
```

**Ověření:** `dotnet build Src/ForgeBlocks/Math/` projde.
**Riziko:** Nízké.
**Rollback:** Odeber projekt, smaž složku.

---

## TASK-8.1.2 — MathForgeBlock registrace

**Vstup:** TASK-8.1.1 (projekt existuje).
**Výstup:** Soubor `Src/ForgeBlocks/Math/MathForgeBlock.cs`.
**Soubory:** `Src/ForgeBlocks/Math/MathForgeBlock.cs`

**Kód:**

```csharp
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.Math;

/// <summary>
/// ForgeBlock pro matematické operace.
/// Poskytuje capabilities: sčítání, odčítání, násobení, dělení, zaokrouhlování.
/// </summary>
public sealed class MathForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "math";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("math_add", "Sčítání", "Sečte dvě čísla", new[] { "math", "arithmetic" }),
        new("math_subtract", "Odčítání", "Odečte dvě čísla", new[] { "math", "arithmetic" }),
        new("math_multiply", "Násobení", "Vynásobí dvě čísla", new[] { "math", "arithmetic" }),
        new("math_divide", "Dělení", "Vydělí dvě čísla", new[] { "math", "arithmetic" }),
        new("math_round", "Zaokrouhlení", "Zaokrouhlí číslo na daný počet desetinných míst", new[] { "math", "rounding" }),
        new("math_abs", "Absolutní hodnota", "Vrátí absolutní hodnotu čísla", new[] { "math" }),
        new("math_pow", "Mocnina", "Umocní číslo na daný exponent", new[] { "math", "power" }),
        new("math_sqrt", "Odmocnina", "Vrátí druhou odmocninu čísla", new[] { "math" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "Math ForgeBlock",
        Description: "Základní matematické operace pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "math", "arithmetic", "rounding" },
        Categories: new[] { "Math", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "math",
        Version: "1.0.0",
        DisplayName: "Math",
        Description: "Matematické operace"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("MathHelper", "MetaForge.ForgeBlocks.Math.MathHelper", "Pomocná třída pro matematické operace"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // Registrace do katalogu — přidá presety pro matematické typy
        // (implementace závisí na CatalogManager — volitelné pro v1)
    }
}
```

**Ověření:** `dotnet build` projde. MathForgeBlock implementuje IForgeBlockCapabilityPackage.
**Riziko:** Nízké.
**Rollback:** Smaž soubor.

---

## TASK-8.2.1 — ForgeBlock.String projekt

**Vstup:** `MetaForge.slnx`.
**Výstup:** Projekt `Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj`.
**Soubory:** `Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj`, `MetaForge.slnx`

**Kód — `MetaForge.ForgeBlocks.String.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.ForgeBlocks.String</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\MetaForge.Core\MetaForge.Core.csproj" />
  </ItemGroup>
</Project>
```

**Aktualizace `MetaForge.slnx`** — přidej:

```xml
    <Project Path="Src/ForgeBlocks/String/MetaForge.ForgeBlocks.String.csproj" />
```

---

## TASK-8.2.2 — StringForgeBlock registrace

**Vstup:** TASK-8.2.1.
**Výstup:** Soubor `Src/ForgeBlocks/String/StringForgeBlock.cs`.
**Soubory:** `Src/ForgeBlocks/String/StringForgeBlock.cs`

**Kód:**

```csharp
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.String;

/// <summary>
/// ForgeBlock pro textové operace.
/// Poskytuje capabilities: konkatenace, formátování, ořezávání, vyhledávání.
/// </summary>
public sealed class StringForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "string";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("str_concat", "Konkatenace", "Spojí dva nebo více řetězců", new[] { "string", "concat" }),
        new("str_format", "Formátování", "Naformátuje řetězec podle šablony", new[] { "string", "format" }),
        new("str_trim", "Ořezání", "Odstraní bílé znaky ze začátku a konce", new[] { "string", "trim" }),
        new("str_contains", "Obsahuje", "Zkontroluje, zda řetězec obsahuje podřetězec", new[] { "string", "search" }),
        new("str_replace", "Nahrazení", "Nahradí část řetězce jiným", new[] { "string", "replace" }),
        new("str_split", "Rozdělení", "Rozdělí řetězec podle oddělovače", new[] { "string", "split" }),
        new("str_upper", "Velká písmena", "Převede řetězec na velká písmena", new[] { "string", "case" }),
        new("str_lower", "Malá písmena", "Převede řetězec na malá písmena", new[] { "string", "case" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "String ForgeBlock",
        Description: "Textové operace pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "string", "text", "format", "search" },
        Categories: new[] { "String", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "string",
        Version: "1.0.0",
        DisplayName: "String",
        Description: "Textové operace"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("StringHelper", "MetaForge.ForgeBlocks.String.StringHelper", "Pomocná třída pro textové operace"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // Registrace do katalogu
    }
}
```

**Ověření:** `dotnet build Src/ForgeBlocks/String/` projde.
**Riziko:** Nízké.
**Rollback:** Smaž oba soubory.

---

## TASK-8.3.1 — ForgeBlock.Validation projekt a registrace

**Vstup:** `MetaForge.slnx`.
**Výstup:** Projekt + registrační soubor pro Validation ForgeBlock.
**Soubory:**
- `Src/ForgeBlocks/Validation/MetaForge.ForgeBlocks.Validation.csproj`
- `Src/ForgeBlocks/Validation/ValidationForgeBlock.cs`

**Kód — `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>MetaForge.ForgeBlocks.Validation</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\MetaForge.Core\MetaForge.Core.csproj" />
  </ItemGroup>
</Project>
```

**Kód — `ValidationForgeBlock.cs`:**

```csharp
using MetaForge.Core.ForgeBlockPackages;

namespace MetaForge.ForgeBlocks.Validation;

/// <summary>
/// ForgeBlock pro validační pravidla.
/// Poskytuje capabilities: not_empty, email_format, range, regex.
/// </summary>
public sealed class ValidationForgeBlock : IForgeBlockCapabilityPackage
{
    public string Handle => "validation";
    public string Version => "1.0.0";

    public IReadOnlyList<ForgeBlockCapability> Capabilities { get; } = new List<ForgeBlockCapability>
    {
        new("val_required", "Povinné pole", "Atribut nesmí být prázdný", new[] { "validation", "required" }),
        new("val_email", "Email formát", "Validace emailové adresy", new[] { "validation", "email" }),
        new("val_phone", "Telefonní formát", "Validace telefonního čísla", new[] { "validation", "phone" }),
        new("val_url", "URL formát", "Validace URL adresy", new[] { "validation", "url" }),
        new("val_range", "Rozsah", "Číselná hodnota v rozsahu", new[] { "validation", "range" }),
        new("val_regex", "Regulární výraz", "Validace podle regex patternu", new[] { "validation", "regex" }),
        new("val_max_length", "Maximální délka", "Maximální délka řetězce", new[] { "validation", "length" }),
        new("val_min_length", "Minimální délka", "Minimální délka řetězce", new[] { "validation", "length" }),
    };

    public DiscoveryMetadata Discovery { get; } = new(
        DisplayName: "Validation ForgeBlock",
        Description: "Validační pravidla pro MetaForge",
        Author: "MetaForge Team",
        Tags: new[] { "validation", "email", "phone", "url", "range", "regex" },
        Categories: new[] { "Validation", "Core" }
    );

    public ForgeBlockPackageDescriptor Descriptor { get; } = new(
        Handle: "validation",
        Version: "1.0.0",
        DisplayName: "Validation",
        Description: "Validační pravidla"
    );

    public IReadOnlyList<ForgeBlockCatalogEntryDescriptor> CatalogEntries { get; } = new List<ForgeBlockCatalogEntryDescriptor>
    {
        new("ValidationHelper", "MetaForge.ForgeBlocks.Validation.ValidationHelper", "Pomocná třída pro validační pravidla"),
    };

    public void Register(ForgeBlockRegistry registry)
    {
        // Registrace do katalogu
    }
}
```

**Aktualizace `MetaForge.slnx`** — přidej:

```xml
    <Project Path="Src/ForgeBlocks/Validation/MetaForge.ForgeBlocks.Validation.csproj" />
```

**Ověření:** `dotnet build Src/ForgeBlocks/Validation/` projde.
**Riziko:** Nízké.
**Rollback:** Smaž soubory.

---

## Souhrn Epic 8 — Co musí existovat po dokončení

```
Src/ForgeBlocks/
├── Math/
│   ├── MetaForge.ForgeBlocks.Math.csproj
│   └── MathForgeBlock.cs
├── String/
│   ├── MetaForge.ForgeBlocks.String.csproj
│   └── StringForgeBlock.cs
└── Validation/
    ├── MetaForge.ForgeBlocks.Validation.csproj
    └── ValidationForgeBlock.cs
```

**Celkem souborů:** ~6
**Build:** Všechny 3 ForgeBlock projekty buildí.

**Checkpoint:** `git tag checkpoint/epic-8-done`
